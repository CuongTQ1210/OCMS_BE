using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Middleware
{
    /// <summary>
    /// Background service để định kỳ kiểm tra và cập nhật trạng thái của các đối tượng trong hệ thống
    /// </summary>
    public class StatusCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StatusCheckBackgroundService> _logger;
        private readonly TimeSpan _checkInterval;

        public StatusCheckBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<StatusCheckBackgroundService> logger,
            TimeSpan? checkInterval = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _checkInterval = checkInterval ?? TimeSpan.FromHours(1); // Mặc định chạy mỗi giờ
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Status check background service started");

            // Đặt lịch chạy vào đầu ngày và giữa ngày
            var nextRunTime = CalculateNextRunTime();
            var delay = nextRunTime - DateTime.Now;

            if (delay.TotalMilliseconds > 0)
            {
                _logger.LogInformation($"Initial delay until next scheduled run: {delay.TotalHours:F1} hours");
                await Task.Delay(delay, stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running scheduled status check at: {time}", DateTime.Now);

                    // Tạo scope để sử dụng các service có lifetime là scoped
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // Check progress tracking statuses (existing functionality)
                        var progressService = scope.ServiceProvider.GetRequiredService<IProgressTrackingService>();
                        await progressService.CheckAndUpdateAllStatuses();
                        
                        // NEW: Check certificate expirations and send notifications
                        var certificateMonitoringService = scope.ServiceProvider.GetRequiredService<ICertificateMonitoringService>();
                        await certificateMonitoringService.CheckAndNotifyExpiringCertificatesAsync();
                    }

                    _logger.LogInformation("Scheduled status check completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during scheduled status check");
                }

                // Tính toán thời gian chạy tiếp theo
                nextRunTime = CalculateNextRunTime(includeToday: false);
                delay = nextRunTime - DateTime.Now;

                _logger.LogInformation($"Next status check scheduled for: {nextRunTime:yyyy-MM-dd HH:mm:ss}");
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("Status check background service stopped");
        }

        /// <summary>
        /// Tính toán thời gian chạy tiếp theo dựa trên lịch đã thiết lập
        /// </summary>
        private DateTime CalculateNextRunTime(bool includeToday = true)
        {
            var now = DateTime.Now;

            // Nếu interval là 1 giờ, chạy mỗi giờ một lần
            if (_checkInterval.TotalHours == 1)
            {
                // Tính giờ kế tiếp (làm tròn lên giờ tiếp theo)
                var nextHour = now.Date.AddHours(now.Hour + 1);
                return nextHour;
            }
            // Nếu interval là 12 giờ, chạy vào 00:00 và 12:00
            else if (_checkInterval.TotalHours == 12)
            {
                var today = now.Date;

                // Thời điểm chạy trong ngày
                var runTime1 = today.AddHours(0); // 00:00
                var runTime2 = today.AddHours(12); // 12:00

                if (includeToday && now < runTime1)
                    return runTime1;
                else if (includeToday && now < runTime2)
                    return runTime2;
                else
                    return today.AddDays(1).AddHours(0); // Ngày tiếp theo, 00:00
            }
            // Nếu interval là 24 giờ, chạy vào 00:00
            else if (_checkInterval.TotalHours == 24)
            {
                var nextDay = now.Date.AddDays(1);
                return nextDay;
            }
            // Các trường hợp khác, chỉ đơn giản cộng thêm khoảng thời gian
            else
            {
                return now.Add(_checkInterval);
            }
        }
    }
}

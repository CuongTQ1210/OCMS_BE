using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OCMS_BOs.Entities;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class CertificateMonitoringService : ICertificateMonitoringService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CertificateMonitoringService> _logger;
        private readonly int _monthsPriorToExpiration = 6; // Default notification period
        private readonly int _maxConcurrentTasks = 10; // Giới hạn số lượng tác vụ đồng thời
        private readonly int _notificationFrequencyDays = 30; // Default frequency for notifications

        public CertificateMonitoringService(
            UnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<CertificateMonitoringService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Check and Notify Expiring Certificates
        /// <summary>
        /// Checks all active certificates and sends notifications for those expiring within the next months
        /// </summary>
        public async Task CheckAndNotifyExpiringCertificatesAsync()
        {
            try
            {
                _logger.LogInformation($"Starting certificate expiration check");

                var today = DateTime.Now;
                var expirationThreshold = today.AddMonths(_monthsPriorToExpiration);

                // Lấy tất cả chứng chỉ sắp hết hạn với eager loading

                var expiringCertificates = await _unitOfWork.CertificateRepository
                    .FindIncludeAsync(c => c.Status == CertificateStatus.Active &&
                                           c.ExpirationDate.HasValue &&
                                           c.ExpirationDate.Value <= expirationThreshold &&
                                           c.ExpirationDate.Value > today,
                                      c => c.User, c => c.Course);

                if (!expiringCertificates.Any())
                {
                    _logger.LogInformation("No certificates expiring within the notification period found");
                    return;
                }

                _logger.LogInformation($"Found {expiringCertificates.Count()} certificates expiring within the next {_monthsPriorToExpiration} months");

                // Lấy tất cả thông báo liên quan trong một truy vấn
                var userIds = expiringCertificates.Select(c => c.UserId).Distinct().ToList();
                var recentNotifications = await _unitOfWork.NotificationRepository
                    .FindAsync(n => userIds.Contains(n.UserId) &&
                                    n.NotificationType == "CertificateExpiration" &&
                                    n.CreatedAt > today.AddDays(-_notificationFrequencyDays));

                // Tạo dictionary để tra cứu nhanh thông báo gần nhất cho mỗi chứng chỉ
                var notificationLookup = recentNotifications
                    .GroupBy(n => n.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Lọc chứng chỉ cần thông báo
                var certificatesToNotify = expiringCertificates
                    .Where(c =>
                    {
                        var userNotifications = notificationLookup.GetValueOrDefault(c.UserId);
                        if (userNotifications == null) return true;

                        var recentNotification = userNotifications
                            .Where(n => n.Title.Contains(c.CertificateCode))
                            .OrderByDescending(n => n.CreatedAt)
                            .FirstOrDefault();

                        if (recentNotification == null) return true;

                        int daysRemaining = (c.ExpirationDate.Value - today).Days;
                        int requiredDays = GetRequiredDaysSinceLastNotification(daysRemaining);
                        return (today - recentNotification.CreatedAt).TotalDays >= requiredDays;
                    })
                    .ToList();

                if (!certificatesToNotify.Any())
                {
                    _logger.LogInformation("All expiring certificates have been recently notified. No new notifications needed.");
                    return;
                }

                _logger.LogInformation($"Sending notifications for {certificatesToNotify.Count()} certificates after filtering");

                // Group certificates by user
                var certificatesByUser = certificatesToNotify
                    .GroupBy(c => c.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                await ProcessNotificationsInParallelAsync(certificatesByUser);

                _logger.LogInformation($"Certificate expiration check completed. Notifications sent to {certificatesByUser.Count} users and their managers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for expiring certificates");
                throw;
            }
        }

        /// <summary>
        /// Xác định số ngày cần đợi trước khi gửi thông báo tiếp theo dựa trên số ngày còn lại
        /// </summary>
        private int GetRequiredDaysSinceLastNotification(int daysRemaining)
        {
            // Ví dụ logic: khoảng cách thông báo phụ thuộc vào số ngày còn lại
            if (daysRemaining <= 30) return 7;  // Thông báo hàng tuần nếu còn dưới 1 tháng
            if (daysRemaining <= 90) return 14; // Thông báo 2 tuần/lần nếu còn dưới 3 tháng
            return _notificationFrequencyDays;  // Mặc định 30 ngày
        }

        /// <summary>
        /// Xử lý song song việc gửi thông báo cho người dùng và manager
        /// </summary>
        private async Task ProcessNotificationsInParallelAsync(Dictionary<string, List<Certificate>> certificatesByUser)
        {
            try
            {
                // Tạo danh sách các tác vụ thông báo
                var notificationTasks = new List<Task>();

                // Sử dụng SemaphoreSlim để giới hạn số lượng tác vụ đồng thời
                using (var throttler = new System.Threading.SemaphoreSlim(_maxConcurrentTasks))
                {
                    foreach (var userCertificates in certificatesByUser)
                    {
                        // Chờ cho đến khi có slot trống cho tác vụ mới
                        await throttler.WaitAsync();

                        // Tạo và khởi chạy tác vụ thông báo song song
                        notificationTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await NotifyUserOfExpiringCertificatesAsync(userCertificates.Key, userCertificates.Value);
                                await NotifyManagerOfExpiringCertificatesAsync(userCertificates.Key, userCertificates.Value);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error processing notifications for user {userCertificates.Key}");
                            }
                            finally
                            {
                                // Giải phóng slot sau khi hoàn thành
                                throttler.Release();
                            }
                        }));
                    }

                    // Đợi tất cả các tác vụ hoàn thành
                    await Task.WhenAll(notificationTasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing parallel notifications");
                throw;
            }
        }
        #endregion

        #region Notification Methods
        /// <summary>
        /// Notifies a user about their expiring certificates
        /// </summary>
        private async Task NotifyUserOfExpiringCertificatesAsync(string userId, List<Certificate> expiringCertificates)
        {
            try
            {
                if (expiringCertificates.Count == 1)
                {
                    // Single certificate expiration notification
                    var certificate = expiringCertificates[0];
                    var remainingDays = (certificate.ExpirationDate.Value - DateTime.Now).Days;
                    var monthsRemaining = Math.Round(remainingDays / 30.0);

                    var title = "Certificate Expiration Notice";
                    var message = $"Your certificate '{certificate.CertificateCode}' for course '{certificate.Course.CourseName}' " +
                                  $"will expire in {(monthsRemaining < 1 ? $"{remainingDays} days" : $"about {monthsRemaining} months")} " +
                                  $"on {certificate.ExpirationDate.Value.ToString("dd/MM/yyyy")}.";

                    await _notificationService.SendNotificationAsync(userId, title, message, "CertificateExpiration");
                    _logger.LogInformation($"Sent single certificate expiration notification to user {userId} for certificate {certificate.CertificateId}");
                }
                else
                {
                    // Xử lý thông báo nhiều chứng chỉ song song
                    await ProcessMultipleCertificateNotificationsAsync(userId, expiringCertificates);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending expiration notification to user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Xử lý song song việc gửi nhiều thông báo chi tiết cho các chứng chỉ
        /// </summary>
        private async Task ProcessMultipleCertificateNotificationsAsync(string userId, List<Certificate> expiringCertificates)
        {
            try
            {
                // Gửi thông báo tổng quan trước
                var title = "Multiple Certificates Expiring Soon";
                var soonestExpiringCert = expiringCertificates.OrderBy(c => c.ExpirationDate).First();
                var furthestExpiringCert = expiringCertificates.OrderByDescending(c => c.ExpirationDate).First();

                var message = $"You have {expiringCertificates.Count} certificates expiring in the next {_monthsPriorToExpiration} months. " +
                              $"The earliest will expire on {soonestExpiringCert.ExpirationDate.Value.ToString("dd/MM/yyyy")} " +
                              $"and the latest on {furthestExpiringCert.ExpirationDate.Value.ToString("dd/MM/yyyy")}.";

                await _notificationService.SendNotificationAsync(userId, title, message, "MultipleCertificateExpiration");
                _logger.LogInformation($"Sent multiple certificate expiration notification to user {userId} for {expiringCertificates.Count} certificates");

                // Gửi song song các thông báo chi tiết
                var detailedNotificationTasks = new List<Task>();

                foreach (var certificate in expiringCertificates)
                {
                    detailedNotificationTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var remainingDays = (certificate.ExpirationDate.Value - DateTime.Now).Days;
                            var monthsRemaining = Math.Round(remainingDays / 30.0);

                            var detailedTitle = "Certificate Expiration Detail";
                            var detailedMessage = $"Certificate '{certificate.CertificateCode}' for course '{certificate.Course.CourseName}' " +
                                                $"will expire in {(monthsRemaining < 1 ? $"{remainingDays} days" : $"about {monthsRemaining} months")} " +
                                                $"on {certificate.ExpirationDate.Value.ToString("dd/MM/yyyy")}.";

                            await _notificationService.SendNotificationAsync(userId, detailedTitle, detailedMessage, "CertificateExpirationDetail");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending detailed notification for certificate {certificate.CertificateId} to user {userId}");
                        }
                    }));
                }

                // Đợi tất cả các thông báo chi tiết hoàn thành
                await Task.WhenAll(detailedNotificationTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing multiple certificate notifications for user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Notifies the AOC Manager about their trainees' expiring certificates
        /// </summary>
        private async Task NotifyManagerOfExpiringCertificatesAsync(string userId, List<Certificate> expiringCertificates)
        {
            try
            {
                // Get the trainee user
                var trainee = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                if (trainee == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found when trying to notify manager");
                    return;
                }

                // Skip if user has no department assigned
                if (string.IsNullOrEmpty(trainee.DepartmentId))
                {
                    _logger.LogInformation($"User {userId} is not assigned to any department, skipping manager notification");
                    return;
                }

                // Find the AOC Manager for this department
                var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(trainee.DepartmentId);
                if (department == null || string.IsNullOrEmpty(department.ManagerUserId))
                {
                    _logger.LogInformation($"Department {trainee.DepartmentId} has no manager assigned, skipping manager notification");
                    return;
                }

                string managerId = department.ManagerUserId;
                var manager = await _unitOfWork.UserRepository.GetByIdAsync(managerId);

                // Verify the manager is an AOC Manager (RoleId 8 based on DepartmentService)
                if (manager == null || manager.RoleId != 8)
                {
                    _logger.LogInformation($"User {managerId} is not an AOC Manager or doesn't exist, skipping notification");
                    return;
                }

                // Send notification to the manager
                if (expiringCertificates.Count == 1)
                {
                    await SendSingleCertificateManagerNotificationAsync(managerId, trainee, expiringCertificates[0]);
                }
                else
                {
                    await SendMultipleCertificateManagerNotificationAsync(managerId, trainee, expiringCertificates);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending manager notification for user {userId}'s expiring certificates");
                // Don't throw here so we don't interrupt the overall process if just the manager notification fails
            }
        }

        /// <summary>
        /// Gửi thông báo cho manager về một chứng chỉ sắp hết hạn
        /// </summary>
        private async Task SendSingleCertificateManagerNotificationAsync(string managerId, User trainee, Certificate certificate)
        {
            var remainingDays = (certificate.ExpirationDate.Value - DateTime.Now).Days;
            var monthsRemaining = Math.Round(remainingDays / 30.0);

            var title = "Trainee Certificate Expiration Notice";
            var message = $"Certificate '{certificate.CertificateCode}' for course '{certificate.Course.CourseName}' " +
                          $"belonging to trainee {trainee.FullName} will expire in " +
                          $"{(monthsRemaining < 1 ? $"{remainingDays} days" : $"about {monthsRemaining} months")} " +
                          $"on {certificate.ExpirationDate.Value.ToString("dd/MM/yyyy")}.";

            await _notificationService.SendNotificationAsync(managerId, title, message, "TraineeCertificateExpiration");
            _logger.LogInformation($"Sent certificate expiration notification to manager {managerId} for trainee {trainee.UserId}'s certificate {certificate.CertificateId}");
        }

        /// <summary>
        /// Gửi thông báo cho manager về nhiều chứng chỉ sắp hết hạn
        /// </summary>
        private async Task SendMultipleCertificateManagerNotificationAsync(string managerId, User trainee, List<Certificate> expiringCertificates)
        {
            // Gửi thông báo tổng quan
            var title = "Multiple Trainee Certificates Expiring Soon";
            var message = $"Trainee {trainee.FullName} has {expiringCertificates.Count} certificates expiring in the next {_monthsPriorToExpiration} months.";

            await _notificationService.SendNotificationAsync(managerId, title, message, "MultipleTraineeCertificateExpiration");
            _logger.LogInformation($"Sent multiple certificate expiration notification to manager {managerId} for trainee {trainee.UserId}'s {expiringCertificates.Count} certificates");

            // Gửi tổng hợp chi tiết
            var summaryTitle = "Trainee Certificate Expiration Details";
            var summaryBuilder = new StringBuilder();
            summaryBuilder.AppendLine($"Certificate expiration details for trainee {trainee.FullName}:");
            summaryBuilder.AppendLine();

            foreach (var certificate in expiringCertificates.OrderBy(c => c.ExpirationDate))
            {
                var remainingDays = (certificate.ExpirationDate.Value - DateTime.Now).Days;
                summaryBuilder.AppendLine($"• Certificate '{certificate.CertificateCode}' for course '{certificate.Course.CourseName}' " +
                                         $"expires on {certificate.ExpirationDate.Value.ToString("dd/MM/yyyy")} " +
                                         $"({remainingDays} days remaining).");
            }

            await _notificationService.SendNotificationAsync(managerId, summaryTitle, summaryBuilder.ToString(), "TraineeCertificateExpirationSummary");
        }
        #endregion

        #region Check and Notify Single Certificate
        /// <summary>
        /// Checks a specific certificate and sends notification if it's expiring soon
        /// </summary>
        public async Task CheckAndNotifySingleCertificateAsync(string certificateId)
        {
            try
            {
                var certificate = await _unitOfWork.CertificateRepository.GetByIdAsync(certificateId);
                if (certificate == null)
                {
                    _logger.LogWarning($"Certificate with ID {certificateId} not found");
                    return;
                }

                // Check if certificate is active and has an expiration date
                if (certificate.Status != CertificateStatus.Active || !certificate.ExpirationDate.HasValue)
                {
                    _logger.LogInformation($"Certificate {certificateId} is not active or has no expiration date");
                    return;
                }

                // Calculate if it's within the notification period
                var today = DateTime.Now;
                var expirationThreshold = today.AddMonths(_monthsPriorToExpiration);

                if (certificate.ExpirationDate.Value <= expirationThreshold && certificate.ExpirationDate.Value > today)
                {
                    // Kiểm tra xem có nên gửi thông báo hay không
                    var cutoffDate = DateTime.Now.AddDays(-GetRequiredDaysSinceLastNotification((certificate.ExpirationDate.Value - today).Days));

                    var recentNotification = await _unitOfWork.NotificationRepository.GetQueryable()
                        .Where(n => n.UserId == certificate.UserId &&
                        n.NotificationType == "CertificateExpiration" &&
                        n.Title.Contains(certificate.CertificateCode) &&
                        n.CreatedAt > cutoffDate)
                        .OrderByDescending(n => n.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (recentNotification == null)
                    {
                        // Xử lý song song với Task.WhenAll
                        await Task.WhenAll(
                            NotifyUserOfExpiringCertificatesAsync(certificate.UserId, new List<Certificate> { certificate }),
                            NotifyManagerOfExpiringCertificatesAsync(certificate.UserId, new List<Certificate> { certificate })
                        );

                        _logger.LogInformation($"Notification sent for certificate {certificateId} to user and manager");
                    }
                    else
                    {
                        _logger.LogInformation($"Certificate {certificateId} was recently notified on {recentNotification.CreatedAt}. Skipping notification.");
                    }
                }
                else
                {
                    _logger.LogInformation($"Certificate {certificateId} is not within the notification period");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking certificate {certificateId} for expiration");
                throw;
            }
        }
        #endregion
    }
}
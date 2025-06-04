using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        #region Send Notification
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDTO notificationDto, string userId)
        {
            if (notificationDto == null)
                return BadRequest("Invalid notification data.");


            await _notificationService.SendNotificationAsync(userId, notificationDto.Title, notificationDto.Message, notificationDto.NotificationType);

            return Ok(new { message = "Notification sent successfully." });
        }
        #endregion

        #region Mask As Read
        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId);
            return Ok(new { message = "Notification marked as read." });
        }
        #endregion

        #region Get Notifications By User Id
        [HttpGet("{userId}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            if (notifications == null || !notifications.Any())
                return NotFound(new { message = "No notifications found for this user." });

            return Ok(new { message = "Notifications retrieved successfully.", data = notifications });
        }
        #endregion

        #region Get unread notifications
        [HttpGet("unread-count/{userId}")]
        [CustomAuthorize]
        public async Task<IActionResult> GetUnreadNotificationCount(string userId)
        {
            try
            {
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                return Ok(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion

        #region Notification Stream
        [HttpGet("stream")]
        [CustomAuthorize]
        public async Task StreamNotifications()
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Response.WriteAsync("data: User ID not found.\n\n");
                await Response.Body.FlushAsync();
                return;
            }
            DateTime lastSent = DateTime.Now;
            while (!Response.HttpContext.RequestAborted.IsCancellationRequested)
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                var newNotifications = notifications.Where(n => n.CreatedAt > lastSent).ToList();

                foreach (var notification in newNotifications)
                {
                    // Thêm id và retry để hỗ trợ reconnect
                    await Response.WriteAsync($"id: {notification.NotificationId}\n");
                    await Response.WriteAsync("retry: 5000\n"); // Thử kết nối lại sau 5 giây
                    await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(notification)}\n\n");
                    await Response.Body.FlushAsync();
                }

                lastSent = DateTime.Now;
                await Task.Delay(1000);
            }
        }
        #endregion
    }
}

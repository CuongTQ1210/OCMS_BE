using OCMS_BOs.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, string type);
        Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(string userId);
        Task MarkNotificationAsReadAsync(int notificationId);

        Task<int> GetUnreadNotificationCountAsync(string userId);
    }
}

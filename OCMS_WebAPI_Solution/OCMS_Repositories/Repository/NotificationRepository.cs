using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class NotificationRepository: GenericRepository<Notification>, INotificationRepository
    {
        private readonly OCMSDbContext _context;
        public NotificationRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> CountAsync(Expression<Func<Notification, bool>> predicate)
        {
            return await _context.Notifications.Where(predicate).CountAsync();
        }
    }
}

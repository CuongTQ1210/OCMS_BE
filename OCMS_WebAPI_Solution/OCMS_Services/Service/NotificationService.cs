using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class NotificationService : INotificationService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(UnitOfWork unitOfWork, IMapper mapper, INotificationRepository notificationRepository, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        #region Send Notification
        public async Task SendNotificationAsync(string userId, string title, string message, string type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.NotificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region Get User Notifications
        public async Task<IEnumerable<NotificationModel>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
            return _mapper.Map<IEnumerable<NotificationModel>>(notifications);
        }
        #endregion

        #region Get User Unread Notification Count
        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            var count = await _notificationRepository.CountAsync(n => n.UserId == userId && !n.IsRead);
            return count;
        }
        #endregion

        #region Mark Notification as Read
        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }
        #endregion

        #region Send Notification after Import Candidate successfully
        public async Task SendCandidateImportNotificationToDirectorsAsync(int successCount)
        {
            var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
            foreach (var director in directors)
            {
                await SendNotificationAsync(
                    director.UserId,
                    "New Candidates Imported",
                    $"{successCount} candidates have been imported and are pending approval.",
                    "CandidateImport"
                );
            }
        }
        #endregion
    }
}

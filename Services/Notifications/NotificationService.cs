using BusinessObject.Notifications;
using Microsoft.AspNetCore.SignalR;
using Repositories.Notifiactions;
using Services.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationRepository = notificationRepository;
            _notificationHubContext = notificationHubContext;
        }

        public async Task SendJobAssignedNotificationAsync(string userId, Guid jobId, string jobName, string serviceName)
        {
            // 1. Lưu vào Database
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"You have been assigned a new job: {jobName} ({serviceName})",
                Type = NotificationType.Message,
                Status = NotificationStatus.Unread,
                Target = $"/jobs/{jobId}",
                TimeSent = DateTime.UtcNow
            };

            await _notificationRepository.CreateNotificationAsync(notification);

            // 2. Gửi real-time qua SignalR (CHỈ GỬI CHO USER NÀY)
            await _notificationHubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    NotificationId = notification.NotificationID,
                    Type = "JOB_ASSIGNED",
                    Title = "New Job Assigned",
                    Content = notification.Content,
                    JobId = jobId,
                    JobName = jobName,
                    ServiceName = serviceName,
                    Target = notification.Target,
                    TimeSent = notification.TimeSent,
                    Status = notification.Status.ToString()
                });

            Console.WriteLine($"[NotificationService] Job assigned notification sent to User_{userId}");
        }

        public async Task SendJobReassignedNotificationAsync(string userId, Guid jobId, string jobName, string serviceName)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"A job has been reassigned to you: {jobName} ({serviceName})",
                Type = NotificationType.Message,
                Status = NotificationStatus.Unread,
                Target = $"/jobs/{jobId}",
                TimeSent = DateTime.UtcNow
            };

            await _notificationRepository.CreateNotificationAsync(notification);

            await _notificationHubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    NotificationId = notification.NotificationID,
                    Type = "JOB_REASSIGNED",
                    Title = "Job Reassigned",
                    Content = notification.Content,
                    JobId = jobId,
                    JobName = jobName,
                    ServiceName = serviceName,
                    Target = notification.Target,
                    TimeSent = notification.TimeSent,
                    Status = notification.Status.ToString()
                });

            Console.WriteLine($"[NotificationService] Job reassigned notification sent to User_{userId}");
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _notificationRepository.GetUnreadNotificationsByUserIdAsync(userId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        // KIỂM TRA QUYỀN TRƯỚC KHI ĐỌC
        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, string userId)
        {
            var ownerId = await _notificationRepository.GetNotificationOwnerIdAsync(notificationId);

            // Chỉ owner mới được đọc
            if (ownerId != userId)
                return false;

            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        // KIỂM TRA QUYỀN TRƯỚC KHI XÓA
        public async Task<bool> DeleteNotificationAsync(Guid notificationId, string userId)
        {
            var ownerId = await _notificationRepository.GetNotificationOwnerIdAsync(notificationId);

            // Chỉ owner mới được xóa
            if (ownerId != userId)
                return false;

            return await _notificationRepository.DeleteNotificationAsync(notificationId);
        }
    }
}

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

        public async Task SendJobDeadlineReminderAsync(string userId, Guid jobId, string jobName, string serviceName, int hoursRemaining)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"Reminder: Job '{jobName}' ({serviceName}) deadline is in {hoursRemaining} hours!",
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
                    Type = "JOB_DEADLINE_REMINDER",
                    Title = "Deadline Reminder",
                    Content = notification.Content,
                    JobId = jobId,
                    JobName = jobName,
                    ServiceName = serviceName,
                    HoursRemaining = hoursRemaining,
                    Target = notification.Target,
                    TimeSent = notification.TimeSent,
                    Status = notification.Status.ToString(),
                    NotificationType = NotificationType.Message.ToString()
                });

            Console.WriteLine($"[NotificationService] Deadline reminder sent to User_{userId}");
        }

        // THÔNG BÁO KHI VỪA QUÁ DEADLINE (Type: Warning)
        public async Task SendJobOverdueWarningAsync(string userId, Guid jobId, string jobName, string serviceName, int hoursOverdue)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"WARNING: Job '{jobName}' ({serviceName}) is overdue by {hoursOverdue} hours!",
                Type = NotificationType.Warning,
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
                    Type = "JOB_OVERDUE",
                    Title = "Job Overdue!",
                    Content = notification.Content,
                    JobId = jobId,
                    JobName = jobName,
                    ServiceName = serviceName,
                    HoursOverdue = hoursOverdue,
                    Target = notification.Target,
                    TimeSent = notification.TimeSent,
                    Status = notification.Status.ToString(),
                    NotificationType = NotificationType.Warning.ToString()
                });

            Console.WriteLine($"[NotificationService] Overdue warning sent to User_{userId}");
        }

        // THÔNG BÁO MỖI NGÀY SAU KHI QUÁ DEADLINE (Type: Warning)
        public async Task SendJobRecurringOverdueWarningAsync(string userId, Guid jobId, string jobName, string serviceName, int daysOverdue)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"URGENT: Job '{jobName}' ({serviceName}) is {daysOverdue} day(s) overdue!",
                Type = NotificationType.Warning,
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
                    Type = "JOB_RECURRING_OVERDUE",
                    Title = $"Job {daysOverdue} Day(s) Overdue!",
                    Content = notification.Content,
                    JobId = jobId,
                    JobName = jobName,
                    ServiceName = serviceName,
                    DaysOverdue = daysOverdue,
                    Target = notification.Target,
                    TimeSent = notification.TimeSent,
                    Status = notification.Status.ToString(),
                    NotificationType = NotificationType.Warning.ToString()
                });

            Console.WriteLine($"[NotificationService] Recurring overdue warning (Day {daysOverdue}) sent to User_{userId}");
        }
        public async Task SendInspectionAssignedNotificationAsync(string userId, Guid inspectionId, string customerConcern, Guid repairOrderId)
        {
            // 1. Lưu vào Database
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"You have been assigned a new inspection: {customerConcern}",
                Type = NotificationType.Message,
                Status = NotificationStatus.Unread,
                Target = $"/inspections/{inspectionId}",
                TimeSent = DateTime.UtcNow
            };

            await _notificationRepository.CreateNotificationAsync(notification);

            // 2. Gửi real-time qua SignalR (NotificationHub)
            await _notificationHubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    NotificationId = notification.NotificationID,
                    Type = "INSPECTION_ASSIGNED",
                    Title = "New Inspection Assigned",
                    Content = notification.Content,
                    InspectionId = inspectionId,
                    CustomerConcern = customerConcern,
                    RepairOrderId = repairOrderId,
                    Target = notification.Target,
                    TimeSent = notification.TimeSent,
                    Status = notification.Status.ToString()
                });

            Console.WriteLine($"[NotificationService] Inspection assigned notification sent to User_{userId}");
        }
    }
}

using BusinessObject.InspectionAndRepair;
using BusinessObject.Notifications;
using Dtos.InspectionAndRepair;
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
        private async Task SendUnreadCountUpdateAsync(string userId)
        {
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

            await _notificationHubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("UnreadCountUpdated", new
                {
                    UserId = userId,
                    UnreadCount = unreadCount,
                    Timestamp = DateTime.UtcNow
                });

            Console.WriteLine($"[NotificationService] Unread count updated: {unreadCount} for User_{userId}");
        }         
        public async Task SendJobAssignedNotificationAsync(string userId, Guid jobId, string jobName, string serviceName)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"You have been assigned a new job: {jobName} ({serviceName})",
                Type = NotificationType.Message,
                Status = NotificationStatus.Unread,
                Target = $"/technician/inspectionAndRepair/repair/repairProgress?id={jobId}",
                TimeSent = DateTime.UtcNow
            };

            await _notificationRepository.CreateNotificationAsync(notification);

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

            await SendUnreadCountUpdateAsync(userId);

            Console.WriteLine($"[NotificationService] Job assigned notification sent to User_{userId}");
        }
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId)
        {
            return await _notificationRepository.GetUnreadNotificationsByUserIdAsync(userId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }
        
        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, string userId)
        {
            var ownerId = await _notificationRepository.GetNotificationOwnerIdAsync(notificationId);

            if (ownerId != userId)
                return false;

            var result = await _notificationRepository.MarkAsReadAsync(notificationId);

            if (result)
            {
                await _notificationHubContext.Clients
                    .Group($"User_{userId}")
                    .SendAsync("NotificationRead", new
                    {
                        NotificationId = notificationId,
                        Status = NotificationStatus.Read.ToString()
                    });

                await SendUnreadCountUpdateAsync(userId);

                Console.WriteLine($"[NotificationService] Notification {notificationId} marked as read for User_{userId}");
            }

            return result;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            var result = await _notificationRepository.MarkAllAsReadAsync(userId);

            if (result)
            {
                await _notificationHubContext.Clients
                    .Group($"User_{userId}")
                    .SendAsync("AllNotificationsRead", new
                    {
                        UserId = userId,
                        Message = "All notifications marked as read",
                        Timestamp = DateTime.UtcNow
                    });

                await SendUnreadCountUpdateAsync(userId);

                Console.WriteLine($"[NotificationService] All notifications marked as read for User_{userId}");
            }

            return result;
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, string userId)
        {
            var ownerId = await _notificationRepository.GetNotificationOwnerIdAsync(notificationId);

            if (ownerId != userId)
                return false;

            var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
            var wasUnread = notification?.Status == NotificationStatus.Unread;

            var result = await _notificationRepository.DeleteNotificationAsync(notificationId);

            if (result)
            {
                await _notificationHubContext.Clients
                    .Group($"User_{userId}")
                    .SendAsync("NotificationDeleted", new
                    {
                        NotificationId = notificationId,
                        Message = "Notification deleted successfully",
                        Timestamp = DateTime.UtcNow
                    });

                if (wasUnread)
                {
                    await SendUnreadCountUpdateAsync(userId);
                }

                Console.WriteLine($"[NotificationService] Notification {notificationId} deleted for User_{userId}");
            }

            return result;
        }       

        public async Task SendInspectionAssignedNotificationAsync(string userId, Guid inspectionId, string customerConcern, Guid repairOrderId)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"You have been assigned a new inspection: {customerConcern}",
                Type = NotificationType.Message,
                Status = NotificationStatus.Unread,
                Target = $"/technician/inspectionAndRepair/inspection/checkVehicle?id={inspectionId}",
                TimeSent = DateTime.UtcNow
            };

            await _notificationRepository.CreateNotificationAsync(notification);

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

            await SendUnreadCountUpdateAsync(userId);

            Console.WriteLine($"[NotificationService] Inspection assigned notification sent to User_{userId}");
        }
        public async Task SendJobOverdueNotificationAsync(string userId, Guid jobId, string jobName, string serviceName, DateTime deadline, int daysOverdue)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = $"Job '{jobName}' is {daysOverdue} day(s) overdue! Deadline was {deadline:dd/MM/yyyy}. Please complete it as soon as possible.",
                Type = NotificationType.Warning,
                Status = NotificationStatus.Unread,
                Target = $"/technician/inspectionAndRepair/repair/repairProgress?id={jobId}",
                TimeSent = DateTime.UtcNow
            };

            await _notificationRepository.CreateNotificationAsync(notification);

            await _notificationHubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    NotificationId = notification.NotificationID,
                    Type = "JOB_OVERDUE",
                    Title = "Job Overdue Alert",
                    Content = notification.Content,
                    JobId = jobId,
                    JobName = jobName,
                    ServiceName = serviceName,
                    Deadline = deadline,
                    DaysOverdue = daysOverdue,
                    Target = notification.Target,
                    TimeSent = notification.TimeSent,
                    Status = notification.Status.ToString()
                });

            await SendUnreadCountUpdateAsync(userId);

            Console.WriteLine($"[NotificationService] Job overdue notification sent to User_{userId} - {daysOverdue} days overdue");
        }
    }
}

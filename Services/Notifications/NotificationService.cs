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
        private readonly Repositories.BranchRepositories.IBranchRepository _branchRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> notificationHubContext,
            Repositories.BranchRepositories.IBranchRepository branchRepository)
        {
            _notificationRepository = notificationRepository;
            _notificationHubContext = notificationHubContext;
            _branchRepository = branchRepository;
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
        //Ham chung cho tat cac ca loại thong bao
        public async Task SendGeneralNotificationAsync(
            string userId,
            string content,
            NotificationType type,
            string target,
            string title = "Notification",
            Dictionary<string, object> metadata = null)
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Content = content,
                Type = type,
                Status = NotificationStatus.Unread,
                Target = target,
                TimeSent = DateTime.UtcNow
            };

            await _notificationRepository.CreateNotificationAsync(notification);

            var payload = new Dictionary<string, object>
            {
                ["NotificationId"] = notification.NotificationID,
                ["Type"] = "GENERAL_NOTIFICATION",
                ["Title"] = title,
                ["Content"] = notification.Content,
                ["Target"] = notification.Target,
                ["TimeSent"] = notification.TimeSent,
                ["Status"] = notification.Status.ToString(),
                ["NotificationType"] = type.ToString()
            };

            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    payload[item.Key] = item.Value;
                }
            }

            await _notificationHubContext.Clients
                .Group($"User_{userId}")
                .SendAsync("ReceiveNotification", payload);

            await SendUnreadCountUpdateAsync(userId);

            Console.WriteLine($"[NotificationService] General notification sent to User_{userId}");
        }

        public async Task SendRepairOrderPaidNotificationToManagersAsync(Guid repairOrderId, Guid branchId, string customerName, string vehicleInfo, decimal amount, string paymentMethod)
        {
            // Get managers from the specific branch
            var managers = await GetManagersByBranchAsync(branchId);

            foreach (var manager in managers)
            {
                var notification = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = manager.Id,
                    Content = $"Mobile payment received: {customerName} paid ${amount:F2} for {vehicleInfo} via {paymentMethod}",
                    Type = NotificationType.Message,
                    Status = NotificationStatus.Unread,
                    Target = $"/manager/repair-orders/{repairOrderId}",
                    TimeSent = DateTime.UtcNow
                };

                await _notificationRepository.CreateNotificationAsync(notification);

                // Send real-time notification via SignalR
                await _notificationHubContext.Clients
                    .Group($"User_{manager.Id}")
                    .SendAsync("ReceiveNotification", new
                    {
                        NotificationId = notification.NotificationID,
                        Type = "MOBILE_PAYMENT_RECEIVED",
                        Title = "Mobile Payment Received",
                        Content = notification.Content,
                        RepairOrderId = repairOrderId,
                        CustomerName = customerName,
                        VehicleInfo = vehicleInfo,
                        Amount = amount,
                        PaymentMethod = paymentMethod,
                        Target = notification.Target,
                        TimeSent = notification.TimeSent,
                        Status = notification.Status.ToString()
                    });

                await SendUnreadCountUpdateAsync(manager.Id);

                Console.WriteLine($"[NotificationService] payment notification sent to Manager_{manager.Id} in Branch_{branchId}");
            }

            Console.WriteLine($"[NotificationService] Mobile payment notifications sent to {managers.Count} managers in Branch_{branchId}");
        }

        public async Task SendRepairOrderCompletedNotificationToManagersAsync(Guid repairOrderId, Guid branchId, string customerName, string vehicleInfo, bool isAutoCompleted)
        {
            // Get managers from the specific branch
            var managers = await GetManagersByBranchAsync(branchId);

            var completionType = isAutoCompleted ? "automatically completed" : "marked as completed";
            var notificationContent = $"Repair order {completionType}: {customerName}'s {vehicleInfo} is ready for payment";

            foreach (var manager in managers)
            {
                var notification = new Notification
                {
                    NotificationID = Guid.NewGuid(),
                    UserID = manager.Id,
                    Content = notificationContent,
                    Type = NotificationType.Message,
                    Status = NotificationStatus.Unread,
                    Target = $"/manager/repair-orders/{repairOrderId}",
                    TimeSent = DateTime.UtcNow
                };

                await _notificationRepository.CreateNotificationAsync(notification);

                // Send real-time notification via SignalR
                await _notificationHubContext.Clients
                    .Group($"User_{manager.Id}")
                    .SendAsync("ReceiveNotification", new
                    {
                        NotificationId = notification.NotificationID,
                        Type = "REPAIR_ORDER_COMPLETED",
                        Title = "Repair Order Completed",
                        Content = notification.Content,
                        RepairOrderId = repairOrderId,
                        CustomerName = customerName,
                        VehicleInfo = vehicleInfo,
                        IsAutoCompleted = isAutoCompleted,
                        CompletionType = completionType,
                        Target = notification.Target,
                        TimeSent = notification.TimeSent,
                        Status = notification.Status.ToString()
                    });

                await SendUnreadCountUpdateAsync(manager.Id);

                Console.WriteLine($"[NotificationService] RO completed notification sent to Manager_{manager.Id} in Branch_{branchId}");
            }

            Console.WriteLine($"[NotificationService] RO completed notifications sent to {managers.Count} managers in Branch_{branchId}");
        }

        private async Task<List<BusinessObject.Authentication.ApplicationUser>> GetManagersByBranchAsync(Guid branchId)
        {
            // Get managers from the specific branch using branch repository
            return await _branchRepository.GetManagersByBranchAsync(branchId);
        }
    }
}

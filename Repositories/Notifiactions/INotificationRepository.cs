using BusinessObject.Notifications;
using Dtos.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Notifiactions
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<List<NotificationDto>> GetNotificationsByUserIdAsync(string userId);
        Task<List<NotificationDto>> GetUnreadNotificationsByUserIdAsync(string userId);
        Task<Notification> GetNotificationByIdAsync(Guid notificationId);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId);
        Task<string> GetNotificationOwnerIdAsync(Guid notificationId);
    }
}

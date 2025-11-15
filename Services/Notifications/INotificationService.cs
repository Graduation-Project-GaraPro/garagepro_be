using BusinessObject.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Notifications
{
    public interface INotificationService
    {
        // Gửi notification
        Task SendJobAssignedNotificationAsync(string userId, Guid jobId, string jobName, string serviceName);
        Task SendJobReassignedNotificationAsync(string userId, Guid jobId, string jobName, string serviceName);       

        // Lấy notification
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);

        // Đọc notification
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);

        // Xóa notification (chỉ owner)
        Task<bool> DeleteNotificationAsync(Guid notificationId, string userId);
    }
}

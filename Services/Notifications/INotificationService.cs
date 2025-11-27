using BusinessObject.Notifications;
using Dtos.InspectionAndRepair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Notifications
{
    public interface INotificationService
    {
        Task SendJobAssignedNotificationAsync(string userId, Guid jobId, string jobName, string serviceName);
        Task SendInspectionAssignedNotificationAsync(string userId, Guid inspectionId, string customerConcern, Guid repairOrderId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId, string userId);

        Task SendJobDeadlineReminderAsync(string userId, Guid jobId, string jobName, string serviceName, int hoursRemaining);
        Task SendJobOverdueWarningAsync(string userId, Guid jobId, string jobName, string serviceName, int hoursOverdue);
        Task SendJobRecurringOverdueWarningAsync(string userId, Guid jobId, string jobName, string serviceName, int daysOverdue);
    }
}

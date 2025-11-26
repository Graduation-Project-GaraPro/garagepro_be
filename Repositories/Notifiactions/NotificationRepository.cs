using BusinessObject.Notifications;
using DataAccessLayer;
using Dtos.InspectionAndRepair;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Notifiactions
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly MyAppDbContext _context;

        public NotificationRepository(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<NotificationDto>> GetNotificationsByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.TimeSent)
                .Select(n => new NotificationDto
                {
                    NotificationID = n.NotificationID,
                    Content = n.Content,
                    Type = n.Type,
                    TimeSent = n.TimeSent,
                    Status = n.Status,
                    Target = n.Target,
                    UserID = n.UserID
                })
                .ToListAsync();
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserID == userId && n.Status == NotificationStatus.Unread)
                .OrderByDescending(n => n.TimeSent)
                .Select(n => new NotificationDto
                {
                    NotificationID = n.NotificationID,
                    Content = n.Content,
                    Type = n.Type,
                    TimeSent = n.TimeSent,
                    Status = n.Status,
                    Target = n.Target,
                    UserID = n.UserID
                })
                .ToListAsync();
        }

        public async Task<Notification> GetNotificationByIdAsync(Guid notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == notificationId);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.Status = NotificationStatus.Read;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserID == userId && n.Status == NotificationStatus.Unread)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Read;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserID == userId && n.Status == NotificationStatus.Unread);
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetNotificationOwnerIdAsync(Guid notificationId)
        {
            var notification = await _context.Notifications
                .Where(n => n.NotificationID == notificationId)
                .Select(n => n.UserID)
                .FirstOrDefaultAsync();

            return notification;
        }
    }
}

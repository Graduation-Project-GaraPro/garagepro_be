using BusinessObject.Enums;
using BusinessObject;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Services.DeadlineServices
{
    public class JobDeadlineService : IJobDeadlineService
    {
        private readonly MyAppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<JobDeadlineService> _logger;

        public JobDeadlineService(
            MyAppDbContext context,
            INotificationService notificationService,
            IJobRepository jobRepository,
            ILogger<JobDeadlineService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        public async Task CheckAndSendDeadlineNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var jobs = await _context.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobTechnicians)
                    .ThenInclude(jt => jt.Technician)
                .Where(j => j.Deadline.HasValue &&
                           j.Status != JobStatus.Completed)
                .ToListAsync();

            foreach (var job in jobs)
            {
                if (!job.Deadline.HasValue) continue;

                var deadline = job.Deadline.Value;
                var timeUntilDeadline = deadline - now;
                var daysUntilDeadline = timeUntilDeadline.TotalDays;

                if (daysUntilDeadline > 0 && daysUntilDeadline <= 1.5)
                {
                    await SendDeadlineReminderAsync(job, daysUntilDeadline);
                }
                else if (daysUntilDeadline < 0 && daysUntilDeadline >= -1) 
                {
                    await SendOverdueWarningAsync(job, Math.Abs(daysUntilDeadline));
                }
                else if (daysUntilDeadline < -1)
                {
                    await SendRecurringOverdueWarningAsync(job, Math.Abs(daysUntilDeadline));
                }
            }

            _logger.LogInformation("Checked {Count} jobs for deadline notifications", jobs.Count);
        }

        private async Task SendDeadlineReminderAsync(Job job, double daysRemaining)
        {
            var notificationKey = $"deadline_reminder_{job.JobId}_{job.Deadline?.Date:yyyyMMdd}";
            if (await WasNotificationSentAsync(notificationKey))
                return;

            var hoursRemaining = (int)(daysRemaining * 24);

            foreach (var jobTechnician in job.JobTechnicians)
            {
                var userId = jobTechnician.Technician?.UserId;
                if (string.IsNullOrEmpty(userId)) continue;

                await _notificationService.SendJobDeadlineReminderAsync(
                    userId,
                    job.JobId,
                    job.JobName,
                    job.Service?.ServiceName ?? "N/A",
                    hoursRemaining
                );
            }

            await MarkNotificationAsSentAsync(notificationKey);
            _logger.LogInformation("Sent deadline reminder for Job {JobId}", job.JobId);
        }

        private async Task SendOverdueWarningAsync(Job job, double daysOverdue)
        {
            var notificationKey = $"overdue_warning_{job.JobId}_{job.Deadline?.Date:yyyyMMdd}";

            if (await WasNotificationSentAsync(notificationKey))
                return;

            var hoursOverdue = (int)(daysOverdue * 24);

            foreach (var jobTechnician in job.JobTechnicians)
            {
                var userId = jobTechnician.Technician?.UserId;
                if (string.IsNullOrEmpty(userId)) continue;

                await _notificationService.SendJobOverdueWarningAsync(
                    userId,
                    job.JobId,
                    job.JobName,
                    job.Service?.ServiceName ?? "N/A",
                    hoursOverdue
                );
            }

            await MarkNotificationAsSentAsync(notificationKey);
            _logger.LogWarning("Sent overdue warning for Job {JobId}", job.JobId);
        }

        private async Task SendRecurringOverdueWarningAsync(Job job, double daysOverdue)
        {
            var dayCount = (int)Math.Floor(daysOverdue);
            var notificationKey = $"recurring_overdue_{job.JobId}_{dayCount}";

            if (await WasNotificationSentAsync(notificationKey))
                return;

            foreach (var jobTechnician in job.JobTechnicians)
            {
                var userId = jobTechnician.Technician?.UserId;
                if (string.IsNullOrEmpty(userId)) continue;

                await _notificationService.SendJobRecurringOverdueWarningAsync(
                    userId,
                    job.JobId,
                    job.JobName,
                    job.Service?.ServiceName ?? "N/A",
                    dayCount
                );
            }

            await MarkNotificationAsSentAsync(notificationKey);
            _logger.LogWarning("Sent recurring overdue warning (Day {Day}) for Job {JobId}", dayCount, job.JobId);
        }

        private async Task<bool> WasNotificationSentAsync(string key)
        {
            return await _context.Notifications
                .AnyAsync(n => n.Target == key && n.TimeSent > DateTime.UtcNow.AddHours(-23));
        }

        private async Task MarkNotificationAsSentAsync(string key)
        {          
        }
    }
}
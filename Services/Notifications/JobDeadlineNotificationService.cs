using BusinessObject.Enums;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Notifications
{
    public class JobDeadlineNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobDeadlineNotificationService> _logger;

        public JobDeadlineNotificationService(
            IServiceProvider serviceProvider,
            ILogger<JobDeadlineNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Job Deadline Notification Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyOverdueJobsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking overdue jobs.");
                }

                var now = DateTime.UtcNow;
                var next8AM = now.Date.AddDays(1).AddHours(8);
                var delay = next8AM - now;

                _logger.LogInformation($"Next check scheduled at {next8AM}. Waiting {delay.TotalHours:F2} hours.");

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task CheckAndNotifyOverdueJobsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var today = DateTime.UtcNow.Date;

            var overdueJobs = await dbContext.Jobs
                .Include(j => j.Service)
                .Include(j => j.JobTechnicians)
                .Where(j => j.Deadline.HasValue
                    && j.Deadline.Value.Date < today
                    && j.Status != JobStatus.Completed)
                .ToListAsync();

            _logger.LogInformation($"Found {overdueJobs.Count} overdue jobs.");

            foreach (var job in overdueJobs)
            {
                var daysOverdue = (today - job.Deadline.Value.Date).Days;

                var technicianUserIds = await dbContext.Technicians
                    .Where(t => job.JobTechnicians.Select(jt => jt.TechnicianId).Contains(t.TechnicianId))
                    .Select(t => t.UserId)
                    .ToListAsync();

                foreach (var userId in technicianUserIds)
                {
                    var alreadyNotifiedToday = await dbContext.Notifications
                        .AnyAsync(n => n.UserID == userId
                            && n.Target == $"/jobs/{job.JobId}"
                            && n.Content.Contains("overdue")
                            && n.TimeSent.Date == today);

                    if (!alreadyNotifiedToday)
                    {
                        await notificationService.SendJobOverdueNotificationAsync(
                            userId,
                            job.JobId,
                            job.JobName,
                            job.Service?.ServiceName ?? "Unknown Service",
                            job.Deadline.Value,
                            daysOverdue
                        );

                        _logger.LogInformation($"Sent overdue notification for Job {job.JobId} to User {userId}. Days overdue: {daysOverdue}");
                    }
                }
            }
        }
    }
}

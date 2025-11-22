using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.DeadlineServices
{
    public class JobDeadlineBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobDeadlineBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); 

        public JobDeadlineBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<JobDeadlineBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Job Deadline Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckJobDeadlinesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking job deadlines.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Job Deadline Background Service is stopping.");
        }

        private async Task CheckJobDeadlinesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var jobDeadlineService = scope.ServiceProvider.GetRequiredService<IJobDeadlineService>();

            _logger.LogInformation("Checking job deadlines at {Time}", DateTime.UtcNow);

            await jobDeadlineService.CheckAndSendDeadlineNotificationsAsync();
        }
    }
}
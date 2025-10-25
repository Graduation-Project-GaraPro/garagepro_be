using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Services.LogServices
{
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LogCleanupService> _logger;

        public LogCleanupService(IServiceProvider serviceProvider, ILogger<LogCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                        logService.CleanupOldLogFiles(1);
                        _logger.LogInformation("Log cleanup completed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during log cleanup");
                }

                // Chạy mỗi 24 giờ
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}

using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Garage_pro_api.BackgroundServices
{
    public class CampaignExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CampaignExpirationService> _logger;

        public CampaignExpirationService(IServiceScopeFactory scopeFactory, ILogger<CampaignExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();

                    var now = DateTime.Now;
                    var utcNow = DateTime.UtcNow;

                    // 🔹 Tìm tất cả chiến dịch đã hết hạn
                    var expiredCampaigns = await context.PromotionalCampaigns
                        .Where(c => c.IsActive && c.EndDate < utcNow)
                        .ToListAsync(stoppingToken);

                    if (expiredCampaigns.Any())
                    {
                        foreach (var campaign in expiredCampaigns)
                        {
                            campaign.IsActive = false;
                            campaign.UpdatedAt = utcNow;
                        }

                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation(
                            "Deactivated {Count} expired campaigns at {Time}.",
                            expiredCampaigns.Count,
                            now);
                    }
                    else
                    {
                        _logger.LogInformation("No expired campaigns found at {Time}.", now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while deactivating expired campaigns.");
                }

                // 🔹 Tính thời gian còn lại đến nửa đêm tiếp theo
                var nextRun = DateTime.Today.AddDays(1);
                var delay = nextRun - DateTime.Now;

                _logger.LogInformation("Next check scheduled in {Hours}h {Minutes}m.",
                    delay.Hours, delay.Minutes);
                Console.WriteLine("delay", delay);

                await Task.Delay(delay, stoppingToken);
            }
        }
    }

}

namespace Garage_pro_api.BackgroundServices
{
    public class CompletedRepairOrderDailyWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CompletedRepairOrderDailyWorker> _logger;
        private readonly TimeZoneInfo _tz;

        public CompletedRepairOrderDailyWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<CompletedRepairOrderDailyWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

           
            _tz = TryGetTimeZone(new[] { "Asia/Bangkok", "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
                  ?? TimeZoneInfo.Local;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _tz);
                    var nextRunLocal = nowLocal.Date.AddDays(1).AddHours(9);
                    var delay = nextRunLocal - nowLocal;

                    if (delay < TimeSpan.FromSeconds(5))
                        delay = TimeSpan.FromSeconds(5);

                    await Task.Delay(delay, stoppingToken);
                    //await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);


                    using var scope = _scopeFactory.CreateScope();

                   
                    var notifier = scope.ServiceProvider
                        .GetRequiredService<ICompletedRepairOrderNotifier>();

                    await notifier.RunOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    
                }
                catch (Exception ex)
                {
                   
                    _logger.LogError(ex, "CompletedRepairOrderDailyWorker crashed but will continue.");

                   
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private static TimeZoneInfo? TryGetTimeZone(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch { /* ignore */ }
            }
            return null;
        }
    }
}

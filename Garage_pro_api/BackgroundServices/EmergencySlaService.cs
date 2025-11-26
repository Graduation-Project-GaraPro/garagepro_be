using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs;

namespace Garage_pro_api.BackgroundServices
{
    public class EmergencySlaService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmergencySlaService> _logger;

        public EmergencySlaService(IServiceScopeFactory scopeFactory, ILogger<EmergencySlaService> logger)
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
                    var hub = scope.ServiceProvider.GetRequiredService<IHubContext<EmergencyRequestHub>>();

                    var now = DateTime.UtcNow;

                    var expired = await context.RequestEmergencies
                        .Include(e => e.Branch)
                        .Include(e => e.Customer)
                        .Where(e => e.Status == BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Pending
                                    && e.ResponseDeadline != null
                                    && e.RespondedAt == null
                                    && e.ResponseDeadline < now)
                        .ToListAsync(stoppingToken);

                    if (expired.Any())
                    {
                        foreach (var e in expired)
                        {
                            e.Status = BusinessObject.RequestEmergency.RequestEmergency.EmergencyStatus.Canceled;
                            e.AutoCanceledAt = now;
                        }

                        await context.SaveChangesAsync(stoppingToken);

                        foreach (var e in expired)
                        {
                            var payload = new
                            {
                                EmergencyRequestId = e.EmergencyRequestId,
                                Status = "Canceled",
                                CustomerId = e.CustomerId,
                                BranchId = e.BranchId,
                                AutoCanceledAt = e.AutoCanceledAt,
                                Message = "Yêu cầu cứu hộ hết thời gian phản hồi",
                                Timestamp = DateTime.UtcNow
                            };

                            await hub.Clients.All.SendAsync("EmergencyRequestExpired", payload, stoppingToken);
                            await hub.Clients.Group($"customer-{e.CustomerId}").SendAsync("EmergencyRequestExpired", payload, stoppingToken);
                            await hub.Clients.Group($"branch-{e.BranchId}").SendAsync("EmergencyRequestExpired", payload, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in EmergencySlaService");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
using BusinessObject.Enums;
using BusinessObject.FcmDataModels;
using DataAccessLayer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Services;
using Services.FCMServices;
using Services.Hubs;

namespace Garage_pro_api.BackgroundServices
{
    public interface ICompletedRepairOrderNotifier
    {
        Task RunOnceAsync(CancellationToken ct);
    }

    public class CompletedRepairOrderNotifier : ICompletedRepairOrderNotifier
    {
        private readonly MyAppDbContext _context;
        private readonly IFcmService _fcmService;
        private readonly IHubContext<JobHub> _hubContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CompletedRepairOrderNotifier> _logger;

        private const int COMPLETED_STATUS_ID = 3;

        public CompletedRepairOrderNotifier(
            MyAppDbContext context,
            IFcmService fcmService,
            IHubContext<JobHub> hubContext,
            IDistributedCache cache,
            ILogger<CompletedRepairOrderNotifier> logger)
        {
            _context = context;
            _fcmService = fcmService;
            _hubContext = hubContext;
            _cache = cache;
            _logger = logger;
        }

        public async Task RunOnceAsync(CancellationToken ct)
        {
          
            var orders = await _context.RepairOrders
                .AsNoTracking()
                .Where(ro =>
                    ro.StatusId == COMPLETED_STATUS_ID &&
                    ro.CompletionDate != null &&
                    ro.IsArchived == false &&
                    ro.IsCancelled == false)
                .Select(ro => new
                {
                    ro.RepairOrderId,
                    ro.UserId,
                    DeviceId = ro.User.DeviceId,   
                    ro.CompletionDate
                })
                .ToListAsync(ct);

            foreach (var ro in orders)
            {
                ct.ThrowIfCancellationRequested();

                var cacheKey = $"notify:ro:completed:{ro.RepairOrderId:N}";
                if (await _cache.GetStringAsync(cacheKey, ct) != null) continue;

                try
                {
                    if (!string.IsNullOrWhiteSpace(ro.DeviceId))
                    {
                        var fcm = new FcmDataPayload
                        {
                            Type = NotificationType.Repair,
                            Title = "Repair Completed",
                            Body = "Your repair order has been completed.\nPlease visit the garage to pick up your vehicle.",

                            EntityKey = EntityKeyType.repairOrderId,
                            EntityId = ro.RepairOrderId,
                            Screen = AppScreen.RepairProgressDetailFragment
                        };

                        await _fcmService.SendFcmMessageAsync(ro.DeviceId, fcm);
                    }

                    await _hubContext.Clients
                        .Group($"RepairOrder_{ro.UserId}")
                        .SendAsync("RepairOrderCompleted", new
                        {
                            repairOrderId = ro.RepairOrderId,
                            statusId = COMPLETED_STATUS_ID,
                            completionDate = ro.CompletionDate
                        }, ct);

                    await _cache.SetStringAsync(
                        cacheKey,
                        "1",
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                            //AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                        },
                        ct);
                }
                catch (Exception ex)
                {
                    
                    _logger.LogError(ex, "Notify failed for RO {RepairOrderId}", ro.RepairOrderId);
                }
            }
        }
    }
}
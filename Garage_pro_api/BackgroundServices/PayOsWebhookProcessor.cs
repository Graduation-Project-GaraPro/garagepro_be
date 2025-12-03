
using System.Text.Json;
using System;
using BusinessObject.PayOsModels;
using BussinessObject;
using DataAccessLayer;
using Dtos.PayOsDtos;
using Microsoft.EntityFrameworkCore;
using Repositories.PaymentRepositories;
using Repositories.WebhookInboxRepositories;
using BusinessObject.Enums;
using BusinessObject.FcmDataModels;
using Microsoft.AspNetCore.SignalR;
using Services.FCMServices;
using Services;
using Services.Hubs;

namespace Garage_pro_api.BackgroundServices
{
    public class PayOsWebhookProcessor : BackgroundService
    {

        private readonly IServiceProvider _sp;
        private readonly ILogger<PayOsWebhookProcessor> _logger;

        
        private const int BatchSize = 100;            
        private const int MaxAttempts = 10;           
        private const int MaxDegreeOfParallelism = 8; 
        private static readonly TimeSpan LoopDelay = TimeSpan.FromMilliseconds(800);

        public PayOsWebhookProcessor(IServiceProvider sp, ILogger<PayOsWebhookProcessor> logger)
        { _sp = sp; _logger = logger; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var gate = new SemaphoreSlim(MaxDegreeOfParallelism);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
                    var webhookInboxRepo = scope.ServiceProvider.GetRequiredService<IWebhookInboxRepository>();

                    // 1) Lấy batch cần xử lý
                    var items = await db.WebhookInboxes
                         .Where(x => (x.Status == WebhookStatus.Pending || x.Status == WebhookStatus.Failed)
                                     && x.Attempts < MaxAttempts)
                         .OrderBy(x => x.ReceivedAt)
                         .Take(50)
                         .ToListAsync(stoppingToken);

                    if (items.Count == 0)
                    {
                        await Task.Delay(LoopDelay, stoppingToken);
                        continue;
                    }

                    // 2) Xử lý song song có kiểm soát
                    var tasks = new List<Task>(items.Count);
                    foreach (var item in items)
                    {
                        await gate.WaitAsync(stoppingToken);
                        tasks.Add(ProcessOneAsync(item, scope, gate, stoppingToken));
                    }

                    await Task.WhenAll(tasks);

                    // 3) Ghi theo lô
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Webhook processor loop failed");
                    await Task.Delay(LoopDelay, stoppingToken);
                }
            }
        }

        private async Task ProcessOneAsync(WebhookInbox item, IServiceScope scope, SemaphoreSlim gate, CancellationToken ct)
        {
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
                var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                // Thêm các service cần cho notify
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<RepairOrderHub>>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

                item.Attempts++;

                var root = JsonDocument.Parse(item.Payload).RootElement;
                var dataEl = root.GetProperty("data");
                var data = new PayOsWebhookData
                {
                    OrderCode = dataEl.GetProperty("orderCode").GetInt64(),
                    Amount = dataEl.TryGetProperty("amount", out var am) ? am.GetDecimal() : 0,
                    Code = dataEl.TryGetProperty("code", out var cd) ? cd.GetString() ?? "" : "",
                    Desc = dataEl.TryGetProperty("desc", out var ds) ? ds.GetString() : null,
                    RawJson = item.Payload,
                    TransactionDateTime = dataEl.TryGetProperty("transactionDateTime", out var dtEl) &&
                        DateTime.TryParse(dtEl.GetString(), out var transactionDate)
                            ? transactionDate
                            : DateTime.Now
                };

                // Idempotent core
                var payment = await repo.GetByConditionAsync(p => p.PaymentId == data.OrderCode, ct);

                if (payment == null)
                {
                    // không thấy payment => tùy bạn đánh Failed hay bỏ qua
                    item.Status = WebhookStatus.Failed;
                    item.LastError = "Payment not found";
                    return;
                }

                var repairOrder = await db.RepairOrders
                    .FirstOrDefaultAsync(r => r.RepairOrderId == payment.RepairOrderId, ct);

                if (payment.Status is not (PaymentStatus.Paid or PaymentStatus.Cancelled))
                {
                    payment.Status = (data.Desc == "success" && data.Code == "00")
                        ? PaymentStatus.Paid
                        : PaymentStatus.Cancelled;

                    if (payment.Status == PaymentStatus.Paid)
                    {
                        payment.PaymentDate = data.TransactionDateTime;
                        if (repairOrder != null)
                        {
                            repairOrder.PaidStatus = PaidStatus.Paid;
                        }

                       
                        if (repairOrder != null)
                        {
                            
                            await hubContext.Clients.All
                                .SendAsync("RepairOrderPaid", repairOrder.RepairOrderId, cancellationToken: ct);

                            
                            // await hubContext.Clients.User(repairOrder.UserId.ToString())
                            //     .SendAsync("RepairOrderPaid", repairOrder.RepairOrderId, cancellationToken: ct);
                        }

                       
                        if (repairOrder != null)
                        {
                            var user = await userService.GetUserByIdAsync(repairOrder.UserId);

                            var fcmNotification = new FcmDataPayload
                            {
                                Type = NotificationType.Order,
                                Title = "Payment Successful",
                                Body = $"Payment for order  is successful.",
                                EntityKey = EntityKeyType.repairOrderId,
                                EntityId = repairOrder.RepairOrderId,
                                Screen = AppScreen.RepairProgressDetailFragment  
                            };

                            await fcmService.SendFcmMessageAsync(user?.DeviceId, fcmNotification);
                        }
                    }

                    payment.ProviderCode = data.Code;
                    payment.ProviderDesc = data.Desc;
                    payment.UpdatedAt = DateTime.UtcNow;

                    await repo.UpdateAsync(payment, ct);

                    if (repairOrder != null)
                    {
                        db.RepairOrders.Update(repairOrder);
                        await db.SaveChangesAsync(ct);
                    }
                }

                item.Status = WebhookStatus.Processed;
                item.ProcessedAt = DateTime.UtcNow;
                item.LastError = null;
            }
            catch (Exception ex)
            {
                item.Status = WebhookStatus.Failed;
                item.LastError = ex.ToString();
            }
            finally
            {
                gate.Release();
            }
        }
    
    }
}


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

namespace Garage_pro_api.BackgroundServices
{
    public class PayOsWebhookProcessor : BackgroundService
    {

        private readonly IServiceProvider _sp;
        private readonly ILogger<PayOsWebhookProcessor> _logger;

        // Tùy chỉnh hiệu năng:
        private const int BatchSize = 100;            // số bản ghi xử lý mỗi vòng
        private const int MaxAttempts = 10;           // retry tối đa
        private const int MaxDegreeOfParallelism = 8; // luồng xử lý song song
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
                        DateTime.TryParse(dtEl.GetString(), out var transactionDate) ?
                        transactionDate : DateTime.Now // hoặc parse từ payload nếu có
                };

                // Idempotent core
                var payment = await repo.GetByConditionAsync(p => p.PaymentId == data.OrderCode, ct);
                var repairOrder = await db.RepairOrders.FirstOrDefaultAsync(r => r.RepairOrderId == payment.RepairOrderId);

                if (payment != null && payment.Status is not (PaymentStatus.Paid or PaymentStatus.Cancelled))
                {
                    payment.Status = data.Code == "00" ? PaymentStatus.Paid : PaymentStatus.Cancelled;
                    if (payment.Status == PaymentStatus.Paid)
                    {
                        payment.PaymentDate = data.TransactionDateTime;
                        repairOrder.PaidStatus = PaidStatus.Paid;
                    }

                    payment.ProviderCode = data.Code;
                    payment.ProviderDesc = data.Desc;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await repo.UpdateAsync(payment, ct);
                }

                item.Status = WebhookStatus.Processed;
                item.ProcessedAt = DateTime.UtcNow;
                item.LastError = null;



            }
            catch (Exception ex)
            {
                // exponential backoff “mềm”: đẩy Failed, lần sau lên lịch tự nhiên theo Attempts
                // (có thể thêm cột NextVisibleAt để backoff chính xác hơn)
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

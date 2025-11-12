

using BusinessObject.PayOsModels;
using BusinessObject;
using BussinessObject;
using DataAccessLayer;
using Dtos.PayOsDtos;
using Microsoft.EntityFrameworkCore;
using Repositories.PaymentRepositories;
using Services.PayOsClients;
using Repositories;
using Microsoft.Extensions.Configuration;

namespace Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;

        private readonly IRepairOrderRepository _repoRepairOrder;
        private readonly IPayOsClient _payos;
        private readonly MyAppDbContext _db;
        private readonly string _baseUrl;

        public PaymentService(IPaymentRepository repo, IPayOsClient payos, MyAppDbContext db, IRepairOrderRepository repoRepairOrder, IConfiguration config)
        {
            _repo = repo;
            _payos = payos;
            _db = db;
            _repoRepairOrder = repoRepairOrder;
            _baseUrl = config["App:BaseUrl"] ?? throw new ArgumentNullException("App:BaseUrl not configured");
        }
        #region CRUD cơ bản
        public Task<Payment?> GetByIdAsync(long paymentId) => _repo.GetByIdAsync(paymentId);

        public Task<IEnumerable<Payment>> GetAllAsync() => _repo.GetAllAsync();

        //public Task AddAsync(Payment payment) => _repo.AddAsync(payment);

        public async Task UpdateAsync(Payment payment , CancellationToken ct)
        {
                await _repo.UpdateAsync(payment, ct);

               await _repo.SaveChangesAsync(ct);
        }

        //public Task DeleteAsync(Guid paymentId) => _repo.DeleteAsync(paymentId);
        #endregion

        #region Truy vấn tiện ích
        public async Task<IEnumerable<Payment>> GetByUserIdAsync(string userId)
        {
            return await _db.Payments
                .Include(p => p.RepairOrder).Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByRepairOrderIdAsync(Guid repairOrderId)
        {
            return await _db.Payments
                .Include(p => p.RepairOrder).Include(p => p.User)
                .Where(p => p.RepairOrderId == repairOrderId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _db.Payments
                .Include(p => p.RepairOrder).Include(p => p.User)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        #endregion

        #region Luồng PayOS
        public async Task<CreatePaymentLinkResult> CreatePaymentAndLinkAsync(CreatePaymentRequest input, string userId, CancellationToken ct = default)
        {
            // 1. Validate input
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // 2. Get repair order and validate ownership
            var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(input.RepairOrderId);

            if (repairOrder == null)
            {
                throw new Exception($"Repair order with ID {input.RepairOrderId} not found");
            }

            if (repairOrder.UserId != userId)
            {
                throw new UnauthorizedAccessException("User does not have permission to access this repair order");
            }

            // 3. Find or create payment record
            var existingPayment = await _repo.GetByConditionAsync(
                p => p.UserId == userId &&
                     p.RepairOrderId == input.RepairOrderId &&
                     p.Status == PaymentStatus.Paid,
                ct);

            Payment payment;

            if (existingPayment != null)
            {
                throw new Exception($"Payment with ID {existingPayment.PaymentId} is paid");

            }
            else
            {
                payment = new Payment
                {
                    RepairOrderId = input.RepairOrderId,
                    UserId = userId,
                    Amount = input.Amount,
                    Method = PaymentMethod.PayOs,
                    Status = PaymentStatus.Unpaid,
                    PaymentDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(payment);
            }

            // 4. Ensure payment is saved and has ID
            if (payment.PaymentId <= 0)
            {
                throw new InvalidOperationException("Payment ID is not generated properly");
            }

            // 5. Prepare URLs for PayOS - FIXED: swapped returnUrl and cancelUrl
            var returnUrl = BuildMyAppUrl(
                hostPath: "success",
                orderCode: payment.PaymentId,
                extra: new Dictionary<string, string>
                {
                    ["amount"] = ((int)Math.Round(input.Amount, MidpointRounding.AwayFromZero)).ToString(),
                    ["transactionId"] = "TX_BE_123"
                });

            //var cancelUrl = BuildMyAppUrl(
            //    hostPath: "cancel",
            //    orderCode: payment.PaymentId,
            //    extra: new Dictionary<string, string>
            //    {
            //        ["reason"] = "user_cancel"
            //    });

            var cancelUrl = $"{_baseUrl}/api/payments/cancel?orderCode={payment.PaymentId}&reason=user_cancel";

            // 6. Create PayOS payment link
            var payOsRequest = new CreatePaymentLinkRequest(
                orderCode: payment.PaymentId,
                amount: (int)Math.Round(input.Amount, MidpointRounding.AwayFromZero),
                description: input.Description ?? $"Payment for repair order {input.RepairOrderId}",
                cancelUrl: cancelUrl,    // Fixed: user cancels -> go to cancelUrl
                returnUrl: returnUrl     // Fixed: payment success -> go to returnUrl
            );

            var payOsResponse = await _payos.CreatePaymentLinkAsync(payOsRequest, ct);

            // 7. Validate PayOS response
            if (payOsResponse?.data == null || string.IsNullOrEmpty(payOsResponse.data.checkoutUrl))
            {
                // Optionally update payment status to failed
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(payment,ct);

                throw new InvalidOperationException($"PayOS returned invalid response: code={payOsResponse?.code}, desc={payOsResponse?.desc}");
            }
            await _repo.SaveChangesAsync(ct);

            // 8. Return result
            return new CreatePaymentLinkResult
            {
                PaymentId = payment.PaymentId,
                OrderCode = payOsResponse.data.orderCode,
                CheckoutUrl = payOsResponse.data.checkoutUrl
               
            };
        }

        //public async Task HandlePayOsWebhookAsync(PayOsWebhookData data, CancellationToken ct = default)
        //{
        //    try {

                
        //        var amount = Convert.ToDecimal(data.Amount);
        //        // ✅ Cách chuẩn: lưu bảng tạm map OrderCode -> PaymentId khi tạo link và tra thẳng ở đây.
        //        var payment = await _repo.GetByConditionAsync(
        //             p => 
        //                  p.Status != PaymentStatus.Paid &&
        //                  p.PaymentId == data.OrderCode
        //             );


        //        if (payment == null)
        //        {
        //            // Không tìm thấy payment khớp — log lại để đối soát, hoặc tạo bản ghi reconciliation.
        //            // Có thể bạn đang dùng orderCode = RepairOrderId (Guid). Nếu đúng, hãy thay logic tra cứu ở đây.
        //            return;
        //        }

        //        if (payment.Status is PaymentStatus.Paid or PaymentStatus.Cancelled)
        //            return; // idempotent

        //        if (data.Code == "00")
        //        {
        //            payment.Status = PaymentStatus.Paid;
        //        }
        //        else
        //        {
        //            payment.Status = PaymentStatus.Cancelled;
        //        }
        //        payment.PaymentDate = data.TransactionDateTime;
        //        payment.ProviderCode = data.Code;
        //        payment.ProviderDesc = data.Desc;
        //        payment.UpdatedAt = DateTime.UtcNow;
        //        await _repo.UpdateAsync(payment);
        //    }
        //    catch(Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}


        private static string BuildMyAppUrl(
            string hostPath,              // "payment/success" hoặc "payment/cancel"
            long orderCode,
            IDictionary<string, string>? extra = null,
            string scheme = "myapp",
            string host = "payment")
        {
            var qp = new List<string> { $"orderCode={orderCode}" };
            if (extra != null)
                qp.AddRange(extra.Select(kv =>
                    $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

            var query = string.Join("&", qp);
            // -> myapp://payment/success?orderCode=123&...
            return $"{scheme}://{host}/{hostPath}?{query}";
        }


        public async Task<PaymentStatusDto> GetStatusByOrderCodeAsync(long orderCode, CancellationToken ct)
        {
            var p = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.PaymentId == orderCode);
            if (p == null) return new PaymentStatusDto { OrderCode = orderCode, Status = PaymentStatus.Unpaid };
            return new PaymentStatusDto
            {
                OrderCode = orderCode,
                Status = p.Status,
                ProviderCode = p.ProviderCode,
                ProviderDesc = p.ProviderDesc,
            };
        }
        #endregion

        #region Đổi trạng thái thủ công
        public async Task MarkPaidAsync(long paymentId, decimal? amount = null, CancellationToken ct = default)
        {
            var payment = await _repo.GetByIdAsync(paymentId) ?? throw new KeyNotFoundException("Payment not found");
            if (amount.HasValue) payment.Amount = amount.Value;
            payment.Status = PaymentStatus.Paid;
            payment.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(payment, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task MarkCancelledAsync(long paymentId, CancellationToken ct = default)
        {
            var payment = await _repo.GetByIdAsync(paymentId) ?? throw new KeyNotFoundException("Payment not found");
            payment.Status = PaymentStatus.Cancelled;
            payment.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(payment, ct);
            await _repo.SaveChangesAsync(ct);

        }
        #endregion


    }
}

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
using BusinessObject.Enums;

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
                if (input.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(input.Amount), "Amount must be > 0");
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
                if (!repairOrder.OrderStatus.StatusName.Equals("Completed"))
                {
                    throw new Exception($"Repair order with ID {input.RepairOrderId} is not Complete");
                }

                if (repairOrder.PaidAmount != input.Amount)
                    throw new Exception("Amount does not match order total");


                var paidPayment = await _repo.GetByConditionAsync(
                 p => p.UserId == userId && p.RepairOrderId == input.RepairOrderId && p.Status == PaymentStatus.Paid, ct);
                if (paidPayment != null) throw new Exception($"Payment {paidPayment.PaymentId} already paid");

                var existingOpen = await _repo.GetByConditionAsync(
                    p => p.UserId == userId && p.RepairOrderId == input.RepairOrderId &&
                         (p.Status == PaymentStatus.Unpaid ), ct);
                if (existingOpen != null)
                    return new CreatePaymentLinkResult { PaymentId = existingOpen.PaymentId, OrderCode = existingOpen.PaymentId, CheckoutUrl = existingOpen.CheckoutUrl };



                var payment = new Payment
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
                await _repo.SaveChangesAsync(ct); 


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


                PayOsResponse<CreatePaymentLinkResponse> payOsResponse;
                try
                {
                    payOsResponse = await _payos.CreatePaymentLinkAsync(payOsRequest, ct);
                }
                catch (Exception ex)
                {
                    // log
                    throw new Exception("PayOS error when creating link", ex);
                }

                // 7. Validate PayOS response
                if (payOsResponse?.data == null || string.IsNullOrEmpty(payOsResponse.data.checkoutUrl))
                {
                    // Optionally update payment status to failed
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _repo.UpdateAsync(payment, ct);
                    await _repo.SaveChangesAsync(ct);

                    throw new InvalidOperationException($"PayOS returned invalid response: code={payOsResponse?.code}, desc={payOsResponse?.desc}");
                }


                payment.Status = PaymentStatus.Unpaid;
                payment.CheckoutUrl = payOsResponse.data.checkoutUrl;
                //payment.ExternalOrderCode = payOsResponse.data.orderCode?.ToString();
                payment.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(payment, ct);
                await _repo.SaveChangesAsync(ct);

                // 8. Return result
                return new CreatePaymentLinkResult
                {
                    PaymentId = payment.PaymentId,
                    OrderCode = payOsResponse.data.orderCode,
                    CheckoutUrl = payOsResponse.data.checkoutUrl

                };
           

        }

       

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

        #region Luồng thanh toán thủ công bởi manager
        public async Task<Payment> CreateManualPaymentAsync(Guid repairOrderId, string managerId, decimal amount, PaymentMethod method, CancellationToken ct = default)
        {
            // 1. Validate input
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be > 0");
            if (method != PaymentMethod.Cash && method != PaymentMethod.PayOs)
                throw new ArgumentException("Invalid payment method for manual payment", nameof(method));

            // 2. Get repair order and validate all jobs are completed
            var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
            if (repairOrder == null)
            {
                throw new Exception($"Repair order with ID {repairOrderId} not found");
            }

            // Check if all jobs are completed
            var allJobsCompleted = repairOrder.Jobs?.All(j => j.Status == BusinessObject.Enums.JobStatus.Completed) ?? false;
            if (!allJobsCompleted)
            {
                throw new Exception($"All jobs in repair order must be completed before payment can be created");
            }

            // 3. Check if there's already a paid payment
            var paidPayment = await _repo.GetByConditionAsync(
                p => p.RepairOrderId == repairOrderId && p.Status == PaymentStatus.Paid, ct);
            if (paidPayment != null) throw new Exception($"Payment {paidPayment.PaymentId} already paid");

            // 4. Create payment record
            var payment = new Payment
            {
                RepairOrderId = repairOrderId,
                UserId = managerId, // Manager ID
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Paid, // Manual payments are immediately marked as paid
                PaymentDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 5. Add payment to database
            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync(ct);

            // 6. Update repair order paid status
            repairOrder.PaidAmount += amount;
            if (repairOrder.PaidAmount >= repairOrder.EstimatedAmount)
            {
                repairOrder.PaidStatus = PaidStatus.Paid;
            }
            else
            {
                // Keep as Unpaid for partial payments since there's no Partial status in the enum
                repairOrder.PaidStatus = PaidStatus.Unpaid;
            }

            await _repoRepairOrder.UpdateAsync(repairOrder);
            await _repoRepairOrder.Context.SaveChangesAsync(ct);

            return payment;
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
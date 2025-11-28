

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
using Repositories.UnitOfWork;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Azure.Core;

namespace Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;

        private readonly IRepairOrderRepository _repoRepairOrder;
        private readonly IPayOsClient _payos;
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyAppDbContext _db;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        public PaymentService(
                IPaymentRepository repo,
                IPayOsClient payos,
                MyAppDbContext db,
                IRepairOrderRepository repoRepairOrder,
                IConfiguration config,
                IUnitOfWork unitOfWork,
                IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _payos = payos;
            _db = db;
            _unitOfWork = unitOfWork;
            _repoRepairOrder = repoRepairOrder;
            _httpContextAccessor = httpContextAccessor;

            var request = _httpContextAccessor.HttpContext?.Request;

            _baseUrl = request is null
                ? config["App:BaseUrl"]                               
                : $"{request.Scheme}://{request.Host}";              
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
        public async Task<CreatePaymentLinkResult> CreatePaymentAndLinkAsync(
            CreatePaymentRequest input,
            string userId,
            CancellationToken ct = default)
        {
            
            await using var trx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Validate input
                if (input == null)
                    throw new ArgumentNullException(nameof(input));

                if (input.Amount <= 0)
                    throw new ArgumentOutOfRangeException(nameof(input.Amount), "Amount must be > 0");

                // 2. Get repair order
                var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(input.RepairOrderId);
                if (repairOrder == null)
                    throw new Exception($"Repair order with ID {input.RepairOrderId} not found");

                if (repairOrder.UserId != userId)
                    throw new UnauthorizedAccessException("User does not have permission to access this repair order");

                if (!repairOrder.OrderStatus.StatusName.Equals("Completed"))
                    throw new Exception($"Repair order with ID {input.RepairOrderId} is not Complete");

                if (repairOrder.Cost != input.Amount)
                    throw new Exception("Amount does not match order total");

                // 3. Check paid payment
                var paidPayment = await _repo.GetByConditionAsync(
                    p => p.UserId == userId &&
                         p.RepairOrderId == input.RepairOrderId &&
                         p.Status == PaymentStatus.Paid, ct);

                if (paidPayment != null)
                    throw new Exception($"Payment {paidPayment.PaymentId} already paid");

                // 4. Existing unpaid?
                var existingOpen = await _repo.GetByConditionAsync(
                    p => p.UserId == userId &&
                         p.RepairOrderId == input.RepairOrderId &&
                         p.Status == PaymentStatus.Unpaid, ct);

                if (existingOpen != null)
                {
                    await trx.CommitAsync(ct);
                    return new CreatePaymentLinkResult
                    {
                        PaymentId = existingOpen.PaymentId,
                        OrderCode = existingOpen.PaymentId,
                        CheckoutUrl = existingOpen.CheckoutUrl
                    };
                }

                // 5. Create new payment
                var payment = new Payment
                {
                    PaymentId= GeneratePaymentId(),
                    RepairOrderId = input.RepairOrderId,
                    UserId = userId,
                    Amount = input.Amount,
                    Method = PaymentMethod.PayOs,
                    Status = PaymentStatus.Unpaid,
                    PaymentDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(payment);
                await _db.SaveChangesAsync(ct);

                if (payment.PaymentId <= 0)
                    throw new InvalidOperationException("Payment ID is not generated properly");

                // 6. Build return/cancel URLs
                var returnUrl = BuildMyAppUrl(
                    hostPath: "success",
                    orderCode: payment.PaymentId,
                    extra: new Dictionary<string, string>
                    {
                        ["amount"] = ((int)Math.Round(input.Amount, MidpointRounding.AwayFromZero)).ToString(),
                        ["transactionId"] = "TX_BE_123"
                    });

                var cancelUrl = $"{_baseUrl}/api/payments/cancel?orderCode={payment.PaymentId}&reason=user_cancel";

                // 7. PayOS request
                var payOsRequest = new CreatePaymentLinkRequest(
                    orderCode: payment.PaymentId,
                    amount: (int)Math.Round(input.Amount, MidpointRounding.AwayFromZero),
                    description: input.Description ?? $"Payment for repair order {input.RepairOrderId}",
                    cancelUrl: cancelUrl,
                    returnUrl: returnUrl
                );

                PayOsResponse<CreatePaymentLinkResponse> payOsResponse;

                try
                {
                    payOsResponse = await _payos.CreatePaymentLinkAsync(payOsRequest, ct);
                }
                catch (Exception ex)
                {
                    await trx.RollbackAsync(ct);
                    throw new Exception("PayOS error when creating link", ex);
                }

                // 8. Validate response
                if (payOsResponse?.data == null ||
                    string.IsNullOrEmpty(payOsResponse.data.checkoutUrl))
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _repo.UpdateAsync(payment, ct);
                    await _db.SaveChangesAsync(ct);

                    await trx.CommitAsync(ct);

                    throw new InvalidOperationException(
                        $"PayOS returned invalid response: code={payOsResponse?.code}, desc={payOsResponse?.desc}"
                    );
                }

                // 9. Save checkout URL
                payment.CheckoutUrl = payOsResponse.data.checkoutUrl;
                payment.UpdatedAt = DateTime.UtcNow;

                await _repo.UpdateAsync(payment, ct);
                await _db.SaveChangesAsync(ct);

                
                await trx.CommitAsync(ct);

                return new CreatePaymentLinkResult
                {
                    PaymentId = payment.PaymentId,
                    OrderCode = payOsResponse.data.orderCode,
                    CheckoutUrl = payOsResponse.data.checkoutUrl
                };
            }
            catch
            {
                await trx.RollbackAsync(ct);
                throw;
            }
        }

        private long GeneratePaymentId()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // 10 digits
            int random = RandomNumberGenerator.GetInt32(10000, 99999);  // 5 digits

            return long.Parse($"{timestamp}{random}"); // total: 15 digits => OK
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

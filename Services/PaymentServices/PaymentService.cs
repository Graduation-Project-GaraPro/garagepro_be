﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using BusinessObject.PayOsModels;
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
using Repositories.UnitOfWork;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Azure.Core;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs;

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
        private readonly IHubContext<RepairOrderHub> _repairOrderHubContext;
        private readonly Services.Notifications.INotificationService _notificationService;

        public PaymentService(
                IPaymentRepository repo,
                IPayOsClient payos,
                MyAppDbContext db,
                IRepairOrderRepository repoRepairOrder,
                IConfiguration config,
                IUnitOfWork unitOfWork,
                IHttpContextAccessor httpContextAccessor,
                IHubContext<RepairOrderHub> repairOrderHubContext,
                Services.Notifications.INotificationService notificationService)
        {
            _repo = repo;
            _payos = payos;
            _db = db;
            _unitOfWork = unitOfWork;
            _repoRepairOrder = repoRepairOrder;
            _httpContextAccessor = httpContextAccessor;
            _repairOrderHubContext = repairOrderHubContext;
            _notificationService = notificationService;

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

        public Task<RepairOrder> GetRepairOrderByIdAsync(Guid repairOrderId)
        {
            return _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
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

        #region Luồng thanh toán thủ công bởi manager
        public async Task<Payment> CreateManualPaymentAsync(Guid repairOrderId, string managerId, decimal amount, PaymentMethod method, CancellationToken ct = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            
            try
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

                if (repairOrder.StatusId != 3)
                {
                    throw new Exception($"Repair order must be in Completed status to process payment. Current status: {repairOrder.OrderStatus?.StatusName ?? "Unknown"}");
                }

                // Check if all jobs are completed
                var allJobsCompleted = repairOrder.Jobs?.All(j => j.Status == BusinessObject.Enums.JobStatus.Completed) ?? false;
                if (!allJobsCompleted)
                {
                    throw new Exception($"All jobs in repair order must be completed before payment can be created");
                }

                // 3. Check if repair order is already fully paid
                if (repairOrder.PaidStatus == PaidStatus.Paid)
                {
                    throw new Exception("Repair order is already fully paid. Cannot create another payment.");
                }

                // 4. Check if there's already a paid payment
                var paidPayment = await _repo.GetByConditionAsync(
                    p => p.RepairOrderId == repairOrderId && p.Status == PaymentStatus.Paid, ct);
                if (paidPayment != null) throw new Exception($"Payment {paidPayment.PaymentId} already paid");

                // 5. Create payment record
                var payment = new Payment
                {
                    PaymentId = GeneratePaymentId(),
                    RepairOrderId = repairOrderId,
                    UserId = repairOrder.UserId,
                    Amount = amount,
                    Method = method,
                    Status = method == PaymentMethod.Cash ? PaymentStatus.Paid : PaymentStatus.Unpaid,
                    PaymentDate = method == PaymentMethod.Cash ? DateTime.UtcNow : DateTime.MinValue,
                    UpdatedAt = DateTime.UtcNow
                };

                // 6. Add payment to database
                await _repo.AddAsync(payment);
                await _repo.SaveChangesAsync(ct);

                // 7. Update repair order paid status
                if (method == PaymentMethod.Cash)
                {
                    // Clear any tracked entities to avoid conflicts
                    _db.ChangeTracker.Clear();
                    
                    // Get fresh instance from database
                    var freshRepairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
                    if (freshRepairOrder != null)
                    {
                        freshRepairOrder.PaidStatus = PaidStatus.Paid;
                        freshRepairOrder.PaidAmount = freshRepairOrder.Cost;
                        await _repoRepairOrder.UpdateAsync(freshRepairOrder);
                    }
                }

                // Commit transaction before sending notifications
                await transaction.CommitAsync(ct);

                // 8. Send SignalR notifications (after successful transaction)
                var paymentNotification = new
                {
                    PaymentId = payment.PaymentId,
                    RepairOrderId = repairOrderId,
                    Amount = amount,
                    Method = method.ToString(),
                    Status = payment.Status.ToString(),
                    PaidStatus = method == PaymentMethod.Cash ? PaidStatus.Paid.ToString() : repairOrder.PaidStatus.ToString(),
                    ProcessedBy = managerId,
                    ProcessedAt = DateTime.UtcNow,
                    CustomerName = repairOrder.User?.FirstName + " " + repairOrder.User?.LastName,
                    VehicleInfo = $"{repairOrder.Vehicle?.Brand?.BrandName} {repairOrder.Vehicle?.Model?.ModelName} ({repairOrder.Vehicle?.LicensePlate})",
                    Message = $"{method} payment created successfully"
                };

                // Notify all managers
                await _repairOrderHubContext.Clients
                    .Group("Managers")
                    .SendAsync("PaymentReceived", paymentNotification);

                // Notify specific repair order watchers
                await _repairOrderHubContext.Clients
                    .Group($"RepairOrder_{repairOrderId}")
                    .SendAsync("PaymentReceived", paymentNotification);

                // Notify customer
                if (!string.IsNullOrEmpty(repairOrder.UserId))
                {
                    await _repairOrderHubContext.Clients
                        .Group($"Customer_{repairOrder.UserId}")
                        .SendAsync("PaymentReceived", paymentNotification);
                }

                // trigger the PayRepairOrder event
                await _repairOrderHubContext.Clients.All
                    .SendAsync("RepairOrderPaid", repairOrderId.ToString());

                // Note: No notification sent for cash payments as managers are already aware of the transaction

                Console.WriteLine($"[PaymentService] {method} payment {payment.PaymentId} created for RO {repairOrderId}");

                return payment;
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync(ct);
                Console.WriteLine($"[PaymentService] Error creating {method} payment for RO {repairOrderId}: {ex.Message}");
                throw;
            }
        }

        public async Task<CreatePaymentLinkResult> CreateManagerPayOsPaymentAsync(Guid repairOrderId, string managerId, string? description = null, CancellationToken ct = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            
            try
            {
                // 1. Get repair order and validate
                var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
            if (repairOrder == null)
            {
                throw new Exception($"Repair order with ID {repairOrderId} not found");
            }

            if (repairOrder.StatusId != 3)
            {
                throw new Exception($"Repair order must be in Completed status to process payment. Current status: {repairOrder.OrderStatus?.StatusName ?? "Unknown"}");
            }

            var allJobsCompleted = repairOrder.Jobs?.All(j => j.Status == BusinessObject.Enums.JobStatus.Completed) ?? false;
            if (!allJobsCompleted)
            {
                throw new Exception($"All jobs in repair order must be completed before payment can be created");
            }

            if (repairOrder.PaidStatus == PaidStatus.Paid)
            {
                throw new Exception("Repair order is already fully paid. Cannot create another payment.");
            }

            var amountToPay = repairOrder.Cost;
            if (amountToPay <= 0)
            {
                throw new Exception("Repair order cost is invalid");
            }

            // check if there's already a paid payment
            var paidPayment = await _repo.GetByConditionAsync(
                p => p.RepairOrderId == repairOrderId && p.Status == PaymentStatus.Paid, ct);
            if (paidPayment != null)
            {
                throw new Exception($"Payment {paidPayment.PaymentId} already paid");
            }

            // Check existing unpaid PayOS payment
            var existingOpen = await _repo.GetByConditionAsync(
                p => p.RepairOrderId == repairOrderId && 
                     p.Method == PaymentMethod.PayOs &&
                     p.Status == PaymentStatus.Unpaid, ct);
            
            if (existingOpen != null)
            {
                // If existing payment has valid CheckoutUrl, return it
                if (!string.IsNullOrEmpty(existingOpen.CheckoutUrl))
                {
                    Console.WriteLine($"[PaymentService] Returning existing payment link for payment {existingOpen.PaymentId}");
                    return new CreatePaymentLinkResult 
                    { 
                        PaymentId = existingOpen.PaymentId, 
                        OrderCode = existingOpen.PaymentId, 
                        CheckoutUrl = existingOpen.CheckoutUrl 
                    };
                }
                
                // If CheckoutUrl is null (PayOS failed before), delete and create new
                Console.WriteLine($"[PaymentService] Existing payment {existingOpen.PaymentId} has null CheckoutUrl, deleting and creating new one");
                _db.Payments.Remove(existingOpen);
                await _db.SaveChangesAsync(ct);
            }

            // 5. Create payment record
            var payment = new Payment
            {
                PaymentId = GeneratePaymentId(),
                RepairOrderId = repairOrderId,
                UserId = repairOrder.UserId, 
                Amount = amountToPay,
                Method = PaymentMethod.PayOs,
                Status = PaymentStatus.Unpaid,
                PaymentDate = DateTime.MinValue,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(payment);
            await _repo.SaveChangesAsync(ct);

            if (payment.PaymentId <= 0)
            {
                throw new InvalidOperationException("Payment ID is not generated properly");
            }

            // URLs for PayOS
            var returnUrl = $"{_baseUrl}/manager/payment/success?orderCode={payment.PaymentId}&repairOrderId={repairOrderId}";
            var cancelUrl = $"{_baseUrl}/api/payments/cancel?orderCode={payment.PaymentId}&reason=user_cancel";

            // Create PayOS payment link
            var payOsRequest = new CreatePaymentLinkRequest(
                orderCode: payment.PaymentId,
                amount: (int)Math.Round(amountToPay, MidpointRounding.AwayFromZero),
                description:$"Payment for order",
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
                throw new Exception("PayOS error when creating link", ex);
            }

            // 9. Validate PayOS response
            if (payOsResponse?.data == null || string.IsNullOrEmpty(payOsResponse.data.checkoutUrl))
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(payment, ct);
                await _repo.SaveChangesAsync(ct);

                throw new InvalidOperationException($"PayOS returned invalid response: code={payOsResponse?.code}, desc={payOsResponse?.desc}");
            }

            // 10. Update payment with checkout URL
            payment.CheckoutUrl = payOsResponse.data.checkoutUrl;
            payment.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(payment, ct);
            await _repo.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);               
                return new CreatePaymentLinkResult
                {
                    PaymentId = payment.PaymentId,
                    OrderCode = payOsResponse.data.orderCode,
                    CheckoutUrl = payOsResponse.data.checkoutUrl
                };
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<PaymentPreviewDto> GetPaymentPreviewAsync(Guid repairOrderId, CancellationToken ct = default)
        {
            var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
            if (repairOrder == null)
            {
                throw new Exception($"Repair order with ID {repairOrderId} not found");
            }

            if (repairOrder.StatusId != 3)
            {
                throw new Exception($"Repair order must be in Completed status to process payment. Current status: {repairOrder.OrderStatus?.StatusName ?? "Unknown"}");
            }

            var preview = new PaymentPreviewDto
            {
                RepairOrderId = repairOrder.RepairOrderId,
                RepairOrderCost = repairOrder.Cost,
                EstimatedAmount = repairOrder.EstimatedAmount,
                PaidAmount = repairOrder.PaidAmount,
                CustomerName = repairOrder.User != null ? $"{repairOrder.User.FirstName} {repairOrder.User.LastName}".Trim() : "Unknown Customer",
                VehicleInfo = repairOrder.Vehicle != null ? $"{repairOrder.Vehicle.Brand?.BrandName ?? "Unknown Brand"} {repairOrder.Vehicle.Model?.ModelName ?? "Unknown Model"} ({repairOrder.Vehicle.LicensePlate ?? "No Plate"})" : "Unknown Vehicle"
            };

            // Calculate discount from quotations
            decimal totalDiscount = 0;
            if (repairOrder.Quotations != null && repairOrder.Quotations.Any())
            {
                // Only include approved quotations
                var approvedQuotations = repairOrder.Quotations.Where(q => q.Status == QuotationStatus.Approved).ToList();
                foreach (var quotation in approvedQuotations)
                {
                    totalDiscount += quotation.DiscountAmount;
                    preview.Quotations.Add(new QuotationInfoDto
                    {
                        QuotationId = quotation.QuotationId,
                        TotalAmount = quotation.TotalAmount,
                        DiscountAmount = quotation.DiscountAmount,
                        Status = quotation.Status.ToString()
                    });
                }
            }
            preview.DiscountAmount = totalDiscount;
            
            // Set TotalAmount as RepairOrderCost minus DiscountAmount
            preview.TotalAmount = preview.RepairOrderCost - preview.DiscountAmount;

            // Get services and parts from approved quotation (customer's selections)
            if (repairOrder.Quotations != null && repairOrder.Quotations.Any())
            {
                // Get the approved quotation (should only be one per repair order)
                var approvedQuotation = repairOrder.Quotations.FirstOrDefault(q => q.Status == QuotationStatus.Approved);
                
                if (approvedQuotation != null && approvedQuotation.QuotationServices != null)
                {
                    // Get selected services from quotation
                    foreach (var quotationService in approvedQuotation.QuotationServices.Where(qs => qs.IsSelected))
                    {
                        if (quotationService.Service != null)
                        {
                            preview.Services.Add(new ServicePreviewDto
                            {
                                ServiceId = quotationService.ServiceId,
                                ServiceName = quotationService.Service.ServiceName,
                                Price = quotationService.Price, // Use price from quotation (locked at time of quote)
                                EstimatedDuration = quotationService.Service.EstimatedDuration
                            });
                        }
                        
                        // Get selected parts for this service from quotation
                        if (quotationService.QuotationServiceParts != null)
                        {
                            foreach (var quotationPart in quotationService.QuotationServiceParts.Where(qsp => qsp.IsSelected))
                            {
                                if (quotationPart.Part != null)
                                {
                                    preview.Parts.Add(new PartPreviewDto
                                    {
                                        PartId = quotationPart.PartId,
                                        PartName = quotationPart.Part.Name,
                                        Quantity = (int)quotationPart.Quantity,
                                        UnitPrice = quotationPart.Price, // Use price from quotation (locked at time of quote)
                                        TotalPrice = quotationPart.Quantity * quotationPart.Price
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Fallback: If no approved quotation, get from repair order services and job parts
                    // This handles cases where payment is made without quotation flow
                    if (repairOrder.RepairOrderServices != null)
                    {
                        var addedServiceIds = new HashSet<Guid>();
                        foreach (var roService in repairOrder.RepairOrderServices)
                        {
                            if (roService.Service != null && !addedServiceIds.Contains(roService.ServiceId))
                            {
                                preview.Services.Add(new ServicePreviewDto
                                {
                                    ServiceId = roService.ServiceId,
                                    ServiceName = roService.Service.ServiceName,
                                    Price = roService.Service.Price,
                                    EstimatedDuration = roService.Service.EstimatedDuration
                                });
                                addedServiceIds.Add(roService.ServiceId);
                            }
                        }
                    }

                    if (repairOrder.Jobs != null)
                    {
                        foreach (var job in repairOrder.Jobs)
                        {
                            if (job.JobParts != null)
                            {
                                foreach (var jobPart in job.JobParts)
                                {
                                    if (jobPart.Part != null)
                                    {
                                        preview.Parts.Add(new PartPreviewDto
                                        {
                                            PartId = jobPart.PartId,
                                            PartName = jobPart.Part.Name,
                                            Quantity = jobPart.Quantity,
                                            UnitPrice = jobPart.UnitPrice,
                                            TotalPrice = jobPart.Quantity * jobPart.UnitPrice
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return preview;
        }

        public async Task<PaymentSummaryDto> GetPaymentSummaryAsync(Guid repairOrderId, CancellationToken ct = default)
        {
            // Get repair order with all related data
            var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(repairOrderId);
            if (repairOrder == null)
            {
                throw new Exception($"Repair order with ID {repairOrderId} not found");
            }

            // Calculate discount from quotations
            decimal totalDiscount = 0;
            if (repairOrder.Quotations != null && repairOrder.Quotations.Any())
            {
                foreach (var quotation in repairOrder.Quotations)
                {
                    totalDiscount += quotation.DiscountAmount;
                }
            }

            // Get payment history
            var paymentHistory = new List<PaymentHistoryDto>();
            if (repairOrder.Payments != null && repairOrder.Payments.Any())
            {
                foreach (var payment in repairOrder.Payments)
                {
                    paymentHistory.Add(new PaymentHistoryDto
                    {
                        PaymentId = payment.PaymentId,
                        Method = payment.Method,
                        Status = payment.Status,
                        Amount = payment.Amount,
                        PaymentDate = payment.PaymentDate,
                        ProcessedBy = payment.User?.UserName ?? "Unknown"
                    });
                }
            }

            var summary = new PaymentSummaryDto
            {
                RepairOrderId = repairOrder.RepairOrderId,
                CustomerName = repairOrder.User?.UserName ?? "Unknown Customer",
                VehicleInfo = $"{repairOrder.Vehicle?.Brand?.BrandName ?? "Unknown Brand"} {repairOrder.Vehicle?.Model?.ModelName ?? "Unknown Model"} ({repairOrder.Vehicle?.LicensePlate ?? "No Plate"})",
                RepairOrderCost = repairOrder.Cost,
                TotalDiscount = totalDiscount,
                AmountToPay = repairOrder.Cost - totalDiscount,
                PaidStatus = repairOrder.PaidStatus,
                PaymentHistory = paymentHistory
            };

            return summary;
        }
        #endregion

        #region Đổi trạng thái thủ công
        public async Task MarkPaidAsync(long paymentId, decimal? amount = null, CancellationToken ct = default)
        {
            // Start database transaction to ensure atomicity
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            
            try
            {
                var payment = await _repo.GetByIdAsync(paymentId) ?? throw new KeyNotFoundException("Payment not found");
                if (amount.HasValue) payment.Amount = amount.Value;
                
                var oldStatus = payment.Status;
                payment.Status = PaymentStatus.Paid;
                payment.PaymentDate = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;
                
                await _repo.UpdateAsync(payment, ct);
                await _repo.SaveChangesAsync(ct);

                // Update repair order paid status
                // Clear any tracked entities to avoid conflicts
                _db.ChangeTracker.Clear();
                
                var repairOrder = await _repoRepairOrder.GetRepairOrderByIdAsync(payment.RepairOrderId);
                if (repairOrder != null)
                {
                    // When payment is marked as paid, update RO to Paid immediately
                    repairOrder.PaidStatus = PaidStatus.Paid;
                    repairOrder.PaidAmount = repairOrder.Cost; // Set to full cost

                    // Update and save (UpdateAsync already calls SaveChangesAsync)
                    await _repoRepairOrder.UpdateAsync(repairOrder);
                }

                // Commit transaction before sending notifications
                await transaction.CommitAsync(ct);

                // Send SignalR notifications
                var paymentNotification = new
                {
                    PaymentId = payment.PaymentId,
                    RepairOrderId = payment.RepairOrderId,
                    Amount = payment.Amount,
                    Method = payment.Method.ToString(),
                    OldStatus = oldStatus.ToString(),
                    NewStatus = "Paid",
                    PaidStatus = repairOrder.PaidStatus.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    CustomerName = repairOrder.User?.FirstName + " " + repairOrder.User?.LastName,
                    VehicleInfo = $"{repairOrder.Vehicle?.Brand?.BrandName} {repairOrder.Vehicle?.Model?.ModelName} ({repairOrder.Vehicle?.LicensePlate})",
                    Message = "Payment confirmed and processed"
                };

                // Notify all managers
                await _repairOrderHubContext.Clients
                    .Group("Managers")
                    .SendAsync("PaymentConfirmed", paymentNotification);

                // Notify specific repair order watchers
                await _repairOrderHubContext.Clients
                    .Group($"RepairOrder_{payment.RepairOrderId}")
                    .SendAsync("PaymentConfirmed", paymentNotification);

                // Notify customer
                if (!string.IsNullOrEmpty(repairOrder.UserId))
                {
                    await _repairOrderHubContext.Clients
                        .Group($"Customer_{repairOrder.UserId}")
                        .SendAsync("PaymentConfirmed", paymentNotification);
                }

                // Also trigger the PayRepairOrder event for board updates
                await _repairOrderHubContext.Clients.All
                    .SendAsync("RepairOrderPaid", payment.RepairOrderId.ToString());

                // Send notification to managers when customer completes mobile payment (PayOS)
                var customerName = $"{repairOrder.User?.FirstName} {repairOrder.User?.LastName}".Trim();
                var vehicleInfo = $"{repairOrder.Vehicle?.Brand?.BrandName} {repairOrder.Vehicle?.Model?.ModelName} ({repairOrder.Vehicle?.LicensePlate})";
                
                await _notificationService.SendRepairOrderPaidNotificationToManagersAsync(
                    payment.RepairOrderId, 
                    repairOrder.BranchId,
                    customerName, 
                    vehicleInfo, 
                    payment.Amount, 
                    payment.Method.ToString()
                );

                Console.WriteLine($"[PaymentService] Payment {paymentId} marked as paid for RO {payment.RepairOrderId}");
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync(ct);
                Console.WriteLine($"[PaymentService] Error marking payment {paymentId} as paid: {ex.Message}");
                throw;
            }
        }

        public async Task MarkCancelledAsync(long paymentId, CancellationToken ct = default)
        {
            // Start database transaction to ensure atomicity
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            
            try
            {
                var payment = await _repo.GetByIdAsync(paymentId) ?? throw new KeyNotFoundException("Payment not found");
                payment.Status = PaymentStatus.Cancelled;
                payment.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(payment, ct);
                await _repo.SaveChangesAsync(ct);

                // Commit transaction
                await transaction.CommitAsync(ct);
                
                Console.WriteLine($"[PaymentService] Payment {paymentId} marked as cancelled");
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync(ct);
                Console.WriteLine($"[PaymentService] Error marking payment {paymentId} as cancelled: {ex.Message}");
                throw;
            }
        }
        #endregion


    }
}
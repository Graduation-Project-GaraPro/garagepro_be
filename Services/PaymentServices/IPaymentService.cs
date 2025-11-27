using BusinessObject;
using BussinessObject;
using Dtos.PayOsDtos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Services.PaymentServices
{
    public interface IPaymentService
    {
        #region CRUD cơ bản
        Task<Payment?> GetByIdAsync(long paymentId);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task UpdateAsync(Payment payment, CancellationToken ct);
        #endregion

        #region Truy vấn tiện ích
        Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Payment>> GetByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<RepairOrder> GetRepairOrderByIdAsync(Guid repairOrderId);
        #endregion

        #region Luồng PayOS
        Task<CreatePaymentLinkResult> CreatePaymentAndLinkAsync(CreatePaymentRequest input, string userId, CancellationToken ct = default);
        Task<PaymentStatusDto> GetStatusByOrderCodeAsync(long orderCode, CancellationToken ct);
        #endregion

        #region Luồng thanh toán thủ công bởi manager
        Task<Payment> CreateManualPaymentAsync(Guid repairOrderId, string managerId, decimal amount, PaymentMethod method, CancellationToken ct = default);
        Task<CreatePaymentLinkResult> CreateManagerPayOsPaymentAsync(Guid repairOrderId, string managerId, string? description = null, CancellationToken ct = default);
        Task<PaymentPreviewDto> GetPaymentPreviewAsync(Guid repairOrderId, CancellationToken ct = default);
        Task<PaymentSummaryDto> GetPaymentSummaryAsync(Guid repairOrderId, CancellationToken ct = default);
        #endregion

        #region Đổi trạng thái thủ công
        Task MarkPaidAsync(long paymentId, decimal? amount = null, CancellationToken ct = default);
        Task MarkCancelledAsync(long paymentId, CancellationToken ct = default);
        #endregion
    }
}
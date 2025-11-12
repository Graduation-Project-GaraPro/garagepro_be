using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BussinessObject;
using Dtos.PayOsDtos;

namespace Services.PaymentServices
{
    public interface IPaymentService
    {

        // CRUD cơ bản
        Task<Payment?> GetByIdAsync(long paymentId);
        Task<IEnumerable<Payment>> GetAllAsync();
        //Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment, CancellationToken ct);
        //Task DeleteAsync(Guid paymentId);

        // Truy vấn tiện ích
        Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Payment>> GetByRepairOrderIdAsync(Guid repairOrderId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);

        Task<PaymentStatusDto> GetStatusByOrderCodeAsync(long orderCode, CancellationToken ct);


        // Luồng thanh toán PayOS
        Task<CreatePaymentLinkResult> CreatePaymentAndLinkAsync(CreatePaymentRequest input, string userId,  CancellationToken ct = default);

        // Xử lý webhook từ PayOS (đã verify chữ ký ở controller)
        //Task HandlePayOsWebhookAsync(PayOsWebhookData data, CancellationToken ct = default);


        // Đổi trạng thái (nếu muốn dùng riêng)
        Task MarkPaidAsync(long paymentId, decimal? amount = null, CancellationToken ct = default);
        Task MarkCancelledAsync(long paymentId, CancellationToken ct = default);

    }
}

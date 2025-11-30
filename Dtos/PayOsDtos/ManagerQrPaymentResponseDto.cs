using System;

namespace Dtos.PayOsDtos
{
    /// <summary>
    /// Response DTO for manager QR payment creation
    /// </summary>
    public class ManagerQrPaymentResponseDto
    {
        public string Message { get; set; } = null!;
        public long PaymentId { get; set; }
        public long OrderCode { get; set; }
        public string CheckoutUrl { get; set; } = null!;
        public string QrCodeUrl { get; set; } = null!;
        public Guid RepairOrderId { get; set; }
        public decimal Amount { get; set; }
    }
}

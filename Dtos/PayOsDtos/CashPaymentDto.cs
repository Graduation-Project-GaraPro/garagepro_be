using System;
using BussinessObject;

namespace Dtos.PayOsDtos
{
    public class CashPaymentDto
    {
        public long PaymentId { get; set; }
        public Guid RepairOrderId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
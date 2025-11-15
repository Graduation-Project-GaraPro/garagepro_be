using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.PayOsDtos
{
    public sealed class CreatePaymentRequest
    {
        public Guid RepairOrderId { get; set; }
        //public string UserId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Description { get; set; } = "Thanh toán đơn sửa chữa";
        //public string? ReturnUrl { get; set; } = null!;
        //public string? CancelUrl { get; set; } = null!;
        //public long? OrderCode { get; set; } 
    }
}

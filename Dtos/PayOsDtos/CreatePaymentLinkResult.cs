using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.PayOsDtos
{
    public sealed class CreatePaymentLinkResult
    {
        public long PaymentId { get; set; }
        public long OrderCode { get; set; }
        public string CheckoutUrl { get; set; } = null!;
    }
}

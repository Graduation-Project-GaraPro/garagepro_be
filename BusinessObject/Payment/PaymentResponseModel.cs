using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObject
{
   public class PaymentResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string OrderDescription { get; set; } = "";
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; } 
        public string TransactionId { get; set; } = "";
        public string Token { get; set; } = "";
        public string VnPayResponseCode { get; set; } = "";

    }
}

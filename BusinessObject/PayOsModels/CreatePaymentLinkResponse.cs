using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.PayOsModels
{
    public class CreatePaymentLinkResponse
    {
        public string id { get; set; } = default!;
        public long orderCode { get; set; }
        public int amount { get; set; }
        public string status { get; set; } = default!;
        public string checkoutUrl { get; set; } = default!; // Tên trường có thể thay đổi, xem response thực tế
    }
}

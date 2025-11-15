using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.PayOsDtos
{
    public sealed class PayOsWebhookData
    {
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; } = ""; // "00" = success (tuỳ kênh)
        public string? Desc { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string RawJson { get; set; } = ""; // lưu log nếu cần
    }
}

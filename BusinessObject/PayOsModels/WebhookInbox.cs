using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.PayOsModels
{
    public class WebhookInbox
    {
        public long Id { get; set; }
        public string Provider { get; set; } = "PayOS";
        public long OrderCode { get; set; }           // để join nhanh sang Payment
        public string Payload { get; set; } = null!;
        public string? Signature { get; set; }

        // 🔹 Dùng enum thay vì string
        public WebhookStatus Status { get; set; } = WebhookStatus.Pending;

        public int Attempts { get; set; } = 0;
        public string? LastError { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }

}

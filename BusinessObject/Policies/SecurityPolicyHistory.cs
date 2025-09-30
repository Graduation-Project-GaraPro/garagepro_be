using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace BusinessObject.Policies
{
    public class SecurityPolicyHistory
    {
        public Guid HistoryId { get; set; } // BIGINT IDENTITY

        // FK đến bảng SecurityPolicies
        public Guid PolicyId { get; set; }
        public virtual SecurityPolicy Policy { get; set; }

        // FK đến Users (người thực hiện thay đổi)
        public string? ChangedBy { get; set; }
        public virtual ApplicationUser ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? ChangeSummary { get; set; }
        public string? PreviousValues { get; set; } // JSON cũ
        public string? NewValues { get; set; }      // JSON mới
    }
}

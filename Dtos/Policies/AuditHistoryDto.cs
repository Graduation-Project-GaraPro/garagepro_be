using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Policies
{
    public class AuditHistoryDto
    {
        public Guid HistoryId { get; set; }
        public Guid PolicyId { get; set; }

        // Có thể bỏ Policy để tránh vòng lặp navigation, hoặc chỉ lấy Id
        public string? Policy { get; set; }

        public string? ChangedBy { get; set; }
        public string? ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; }
        public string? ChangeSummary { get; set; }
        public string? PreviousValues { get; set; }
        public string? NewValues { get; set; }
    }
}

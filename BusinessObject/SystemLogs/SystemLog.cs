using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;

namespace BusinessObject.SystemLogs
{
    public class SystemLog
    {
        [Key]
        public long Id { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public LogLevel Level { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        [MaxLength(255)]
        public string Source { get; set; }

        public string Message { get; set; }

        public string Details { get; set; }
        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }

        [MaxLength(255)]
        public string UserName { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        [MaxLength(100)]
        public string SessionId { get; set; }

        [MaxLength(100)]
        public string RequestId { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
        public LogCategory Category { get; set; }
        public ICollection<LogTag>? Tags { get; set; }
        public ICollection<SecurityLogRelation> SecurityLogRelations { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Authentication;
using Microsoft.Extensions.Logging;

namespace BusinessObject.SystemLogs
{
    public class SystemLog
    {
        [Key]
        public long Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public LogTagType? Tag { get; set; }

        
        public LogSource Source { get; set; }
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

        public ApplicationUser? User { get; set; }
        
    }
}

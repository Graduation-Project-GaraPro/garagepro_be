using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos.Logs
{
    public class SystemLogDto
    {
        public long Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details{ get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using Microsoft.Extensions.Logging;

namespace Dtos.Logs
{
    public class LogSearchRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public List<LogLevel>? Levels { get; set; }
        public List<LogSource>? Sources { get; set; }
        public string? SearchTerm { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int? Days { get; set; } // Số ngày để lấy log từ file
    }
}

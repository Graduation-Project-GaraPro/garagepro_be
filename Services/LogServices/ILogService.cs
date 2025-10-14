using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;

namespace Services.LogServices
{
    public interface ILogService
    {
        Task LogSystemAsync(LogLevel level, string message, string? source = null, string? details = null);
        Task LogUserActivityAsync(string userId, string userName, string action, string ip, string? userAgent, bool isSensitive = false);
        Task<IEnumerable<SystemLog>> GetUserActivityLogsAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using Microsoft.Extensions.Logging;

namespace Services
{
    public interface ISystemLogService
    {
        Task<IEnumerable<SystemLog>> GetLogsAsync(LogLevel level, DateTimeOffset? start, DateTimeOffset? end, string? userId);
        Task<SystemLog?> GetLogAsync(long id);
        Task<SystemLog> CreateLogAsync(SystemLog log);
        Task<bool> DeleteLogAsync(long id);
    }
}

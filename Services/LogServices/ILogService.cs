using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using Dtos.Logs;
using Microsoft.Extensions.Logging;

namespace Services.LogServices
{

    public interface ILogService
    {
        // Ghi log
        Task LogSystemAsync(string message, LogLevel level = LogLevel.Information);
        Task LogSecurityAsync(string message, string? userId = null);
        Task LogUserActivityAsync(string action, string userId, string userName);
        Task LogApiAsync(string controller, string action, string? userId = null);
        Task LogDatabaseAsync(string operation, string? table = null, LogLevel level = LogLevel.Information, string? details = null);
        Task LogErrorAsync(Exception ex, string? message = null);

        // Đọc log
        Task<IEnumerable<SystemLog>> GetUserActivityLogsAsync();
        Task<IEnumerable<SystemLogDto>> GetAllSystemLogsAsync();
        Task<IEnumerable<SystemLogDto>> GetLogsBySourceAsync(LogSource source);
        Task<IEnumerable<SystemLogDto>> GetLogsFromFileAsync(LogSource source, int days = 7);
        Task<LogSearchResult> GetAllLogsAsync(LogSearchRequest request);

        Task<LogStatistics> GetLogStatistics(int days = 7);
        // Quản lý
        void CleanupOldLogFiles(int retentionDays = 30);
    }
}


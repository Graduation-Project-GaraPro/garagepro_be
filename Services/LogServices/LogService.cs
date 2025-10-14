using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.SystemLogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.LogRepositories;
using System.Text.Json;
namespace Services.LogServices
{
    public class LogService : ILogService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemLogRepository _logRepository;
        private readonly string _activityLogPath;

        public LogService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, ISystemLogRepository logRepository)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _logRepository = logRepository;

            var logDir = Path.Combine(_env.ContentRootPath, "Logs");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

            _activityLogPath = Path.Combine(logDir, "activity-log.jsonl");
        }

        public async Task LogSystemAsync(BusinessObject.SystemLogs.LogLevel level, string message, string? source = null, string? details = null)
        {
            var systemLog = $"{DateTimeOffset.UtcNow:u} [{level}] {source ?? "System"}: {message}";
            var logFilePath = Path.Combine(_env.ContentRootPath, "Logs", "app-log.txt");
            await File.AppendAllTextAsync(logFilePath, systemLog + Environment.NewLine);
        }

        public async Task LogUserActivityAsync(string userId, string userName, string action, string ip, string? userAgent, bool isSensitive = false)
        {
            var logEntry = new SystemLog
            {
                Timestamp = DateTimeOffset.UtcNow,
                Level = BusinessObject.SystemLogs.LogLevel.Info,
                UserId = userId,
                UserName = userName,
                Message = action,
                IpAddress = ip,
                UserAgent = userAgent,
                Source = "UserActivity"
            };

            var jsonLine = JsonSerializer.Serialize(logEntry);
            await File.AppendAllTextAsync(_activityLogPath, jsonLine + Environment.NewLine);

            if (isSensitive)
            {
                await _logRepository.AddAsync(logEntry);
            }
        }

        public async Task<IEnumerable<SystemLog>> GetUserActivityLogsAsync()
        {
            if (!File.Exists(_activityLogPath))
                return Enumerable.Empty<SystemLog>();

            var lines = await File.ReadAllLinesAsync(_activityLogPath);
            var logs = new List<SystemLog>();

            foreach (var line in lines)
            {
                try
                {
                    var entry = JsonSerializer.Deserialize<SystemLog>(line);
                    if (entry != null)
                        logs.Add(entry);
                }
                catch { /* skip malformed lines */ }
            }

            return logs.OrderByDescending(l => l.Timestamp);
        }
    }
}

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
using Dtos.Logs;
using Microsoft.AspNetCore.SignalR;
namespace Services.LogServices
{
    public class LogService : ILogService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemLogRepository _logRepository;
        private readonly Dictionary<LogSource, string> _logFilePaths;

        private readonly IHubContext<LogHub> _hubContext;
        private readonly long _maxFileSizeBytes = 10 * 1024 * 1024; // 10MB
        private readonly int _maxBackupFiles = 3;

        public LogService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, ISystemLogRepository logRepository, IHubContext<LogHub> hubContext)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _logRepository = logRepository;
            _hubContext = hubContext;
            _logFilePaths = new Dictionary<LogSource, string>
            {
                [LogSource.System] = Path.Combine(env.ContentRootPath, "Logs", "system.log"),
                [LogSource.Security] = Path.Combine(env.ContentRootPath, "Logs", "security.log"),
                [LogSource.UserActivity] = Path.Combine(env.ContentRootPath, "Logs", "user-activity.log"),
                [LogSource.Middleware] = Path.Combine(env.ContentRootPath, "Logs", "middleware.log"),
                [LogSource.Authentication] = Path.Combine(env.ContentRootPath, "Logs", "authentication.log"),
                [LogSource.ApiController] = Path.Combine(env.ContentRootPath, "Logs", "api.log"),
                [LogSource.Database] = Path.Combine(env.ContentRootPath, "Logs", "database.log")
            };

            Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "Logs"));
        }

        // Ghi log
        public async Task LogSystemAsync(string message, LogLevel level = LogLevel.Information)
            => await WriteLogAsync(LogSource.System, level, message);

        public async Task LogSecurityAsync(string message, string? userId = null)
            => await WriteLogAsync(LogSource.Security, LogLevel.Warning, message, userId);

        public async Task LogUserActivityAsync(string action, string userId, string userName)
            => await WriteLogAsync(LogSource.UserActivity, LogLevel.Information, action, userId);

        public async Task LogApiAsync(string controller, string action, string? userId = null)
            => await WriteLogAsync(LogSource.ApiController, LogLevel.Information, $"{controller}.{action}", userId);

        public async Task LogDatabaseAsync(string operation, string? table = null, LogLevel level = LogLevel.Information, string? details = null)
            => await WriteLogAsync(LogSource.Database, level, table != null ? $"{operation} on {table}" : operation, details: details);

        public async Task LogErrorAsync(Exception ex, string? message = null)
            => await WriteLogAsync(LogSource.System, LogLevel.Error, message ?? ex.Message, details: ex.StackTrace);

        // Method chung ghi log
        private async Task WriteLogAsync(LogSource source, LogLevel level, string message, string? userId = null, string? details = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var logEntry = new SystemLog
            {
                Timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)),
                Level = level,
                Source = source,
                UserId = userId,
                UserName = context?.User.Identity?.Name ?? "Anonymous",
                IpAddress = context?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Message = message,
                Details = details,
                UserAgent = context?.Request.Headers["User-Agent"].ToString() ?? "N/A",
                RequestId = context?.TraceIdentifier ?? Guid.NewGuid().ToString()
            };

            // Ghi file

            // Ghi database nếu quan trọng
            if (level >= LogLevel.Warning || source == LogSource.Database)
            {
                await _logRepository.AddAsync(logEntry);
            }else
            {
                await WriteToFileAsync(source, logEntry);
            }
            // Gửi realtime notification qua SignalR
            await SendLogNotificationAsync(logEntry);
        }
        private async Task SendLogNotificationAsync(SystemLog log)
        {
            try
            {
                var logDto = MapToDto(log);

                // Gửi đến tất cả clients đang kết nối
                await _hubContext.Clients.All.SendAsync("ReceiveNewLog", logDto);

                // Gửi đến group cụ thể theo level (tuỳ chọn)
                await _hubContext.Clients.Group($"level-{log.Level}").SendAsync("ReceiveNewLog", logDto);

                // Gửi đến group cụ thể theo source (tuỳ chọn)
                await _hubContext.Clients.Group($"source-{log.Source}").SendAsync("ReceiveNewLog", logDto);

                // Gửi thông báo cập nhật thống kê
                await _hubContext.Clients.All.SendAsync("UpdateStats");
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không làm gián đoạn quá trình ghi log
                await WriteToFileAsync(LogSource.System, new SystemLog
                {
                    Timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)),
                    Level = LogLevel.Error,
                    Source = LogSource.System,
                    Message = $"Failed to send log notification: {ex.Message}",
                    UserName = "System",
                    IpAddress = "127.0.0.1"
                });
            }
        }
        // Ghi file
        private async Task WriteToFileAsync(LogSource source, SystemLog log)
        {
            var filePath = _logFilePaths[source];
            await RotateLogFileIfNeeded(filePath);

            var logLine = $"{log.Timestamp:yyyy-MM-dd HH:mm:ss} [{log.Level}] {log.Message} " +
                         $"(User: {log.UserName}, IP: {log.IpAddress})" +
                         (!string.IsNullOrEmpty(log.Details) ? $"\nDetails: {log.Details}" : "");

            await File.AppendAllTextAsync(filePath, logLine + Environment.NewLine);
        }

        // Xoay file
        private async Task RotateLogFileIfNeeded(string filePath)
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length < _maxFileSizeBytes)
                return;

            await Task.Delay(100);

            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            // Xóa file cũ nhất
            var oldestBackup = Path.Combine(directory, $"{fileName}.{_maxBackupFiles}{extension}");
            if (File.Exists(oldestBackup))
                File.Delete(oldestBackup);

            // Dịch chuyển các file backup
            for (int i = _maxBackupFiles - 1; i > 0; i--)
            {
                var oldFile = Path.Combine(directory, $"{fileName}.{i}{extension}");
                if (File.Exists(oldFile))
                {
                    var newFile = Path.Combine(directory, $"{fileName}.{i + 1}{extension}");
                    File.Move(oldFile, newFile, true);
                }
            }

            // Di chuyển file hiện tại thành backup.1
            var firstBackup = Path.Combine(directory, $"{fileName}.1{extension}");
            File.Move(filePath, firstBackup, true);
        }

        // Đọc log
        public async Task<IEnumerable<SystemLog>> GetUserActivityLogsAsync()
            => await _logRepository.GetBySourceAsync(LogSource.UserActivity);

        public async Task<IEnumerable<SystemLogDto
            
            >> GetAllSystemLogsAsync()
            => (await _logRepository.GetAllAsync()).Select(MapToDto);

        public async Task<IEnumerable<SystemLogDto>> GetLogsBySourceAsync(LogSource source)
            => (await _logRepository.GetBySourceAsync(source)).Select(MapToDto);

        public async Task<IEnumerable<SystemLogDto>> GetLogsFromFileAsync(LogSource source, int days = 7)
        {
            var filePath = _logFilePaths[source];
            if (!File.Exists(filePath)) return Enumerable.Empty<SystemLogDto>();

            var lines = await File.ReadAllLinesAsync(filePath);
            var logs = new List<SystemLogDto>();

            foreach (var line in lines)
            {
                var log = ParseLogLine(line, source);
                if (log != null && log.Timestamp >= DateTimeOffset.Now.AddDays(-days))
                    logs.Add(log);
            }

            return logs.OrderByDescending(x => x.Timestamp);
        }
        public async Task<LogSearchResult> GetAllLogsAsync(LogSearchRequest request)
        {
            var result = new LogSearchResult();
            var allLogs = new List<SystemLogDto>();

            // Lấy log từ database
            var dbLogs = await _logRepository.GetAllAsync();

            // Filter log từ database
            var filteredDbLogs = dbLogs.AsQueryable();

            if (request.Levels != null && request.Levels.Any())
            {
                filteredDbLogs = filteredDbLogs.Where(log => request.Levels.Contains(log.Level));
            }

            if (request.Sources != null && request.Sources.Any())
            {
                filteredDbLogs = filteredDbLogs.Where(log => request.Sources.Contains(log.Source));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                filteredDbLogs = filteredDbLogs.Where(log =>
                    log.Message.Contains(searchTerm) ||
                    (log.UserName != null && log.UserName.Contains(searchTerm)) ||
                    (log.UserId != null && log.UserId.Contains(searchTerm)) ||
                    (log.IpAddress != null && log.IpAddress.Contains(searchTerm)) ||
                    (log.Details != null && log.Details.Contains(searchTerm)));
            }

            // CHỈ filter theo thời gian nếu có giá trị
            if (request.StartDate.HasValue)
            {
                filteredDbLogs = filteredDbLogs.Where(log => log.Timestamp >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                filteredDbLogs = filteredDbLogs.Where(log => log.Timestamp <= request.EndDate.Value);
            }

            // Map to DTO và thêm vào danh sách
            allLogs.AddRange(filteredDbLogs.Select(MapToDto));

            // Lấy log từ file - mặc định 365 ngày nếu không có filter thời gian
            var daysToRead = request.Days ?? (request.StartDate.HasValue ?
                (DateTime.Now - request.StartDate.Value).Days + 7 : 365);

            // Đảm bảo không vượt quá giới hạn
            daysToRead = Math.Min(daysToRead, 365);

            var sourcesToRead = request.Sources ?? Enum.GetValues<LogSource>().ToList();

            foreach (var source in sourcesToRead)
            {
                try
                {
                    var fileLogs = await GetLogsFromFileAsync(source, daysToRead);

                    // Filter log từ file (tương tự như database)
                    var filteredFileLogs = fileLogs.AsQueryable();

                    if (request.Levels != null && request.Levels.Any())
                    {
                        filteredFileLogs = filteredFileLogs.Where(log =>
                            request.Levels.Any(level =>
                                string.Equals(level.ToString(), log.Level, StringComparison.OrdinalIgnoreCase)));
                    }

                    if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    {
                        var searchTerm = request.SearchTerm.Trim();
                        filteredFileLogs = filteredFileLogs.Where(log =>
                            log.Message.Contains(searchTerm) ||
                            (log.UserName != null && log.UserName.Contains(searchTerm)) ||
                            (log.UserId != null && log.UserId.Contains(searchTerm)) ||
                            (log.IpAddress != null && log.IpAddress.Contains(searchTerm)) ||
                            (log.Details != null && log.Details.Contains(searchTerm)));
                    }

                    // CHỈ filter theo thời gian nếu có giá trị
                    if (request.StartDate.HasValue)
                    {
                        filteredFileLogs = filteredFileLogs.Where(log => log.Timestamp >= request.StartDate.Value);
                    }

                    if (request.EndDate.HasValue)
                    {
                        filteredFileLogs = filteredFileLogs.Where(log => log.Timestamp <= request.EndDate.Value);
                    }

                    allLogs.AddRange(filteredFileLogs);
                }
                catch (Exception ex)
                {
                    await WriteLogAsync(LogSource.System, LogLevel.Error,
                        $"Error reading log file for {source}: {ex.Message}");
                }
            }

            // Sắp xếp và phân trang
            var sortedLogs = allLogs.OrderByDescending(log => log.Timestamp);

            result.TotalCount = sortedLogs.Count();
            result.PageNumber = request.PageNumber;
            result.PageSize = request.PageSize;
            result.Logs = sortedLogs
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return result;
        }


        public async Task<LogStatistics> GetLogStatistics(int days = 7)
        {
            try
            {
                if (days < 1 || days > 365)
                    days = 7;

                var cutoffDate = DateTimeOffset.UtcNow.AddDays(-days);

                // Get logs from DB
                var dbLogs = await _logRepository.GetAllAsync();
                var recentLogs = dbLogs.Where(log => log.Timestamp >= cutoffDate).ToList();

                // Get logs from files
                var fileLogs = new List<SystemLogDto>();
                foreach (var source in Enum.GetValues<LogSource>())
                {
                    try
                    {
                        var logsFromFile = await GetLogsFromFileAsync(source, days);
                        fileLogs.AddRange(logsFromFile.Where(log => log.Timestamp >= cutoffDate));
                    }
                    catch
                    {
                        // Ignore file reading errors
                    }
                }

                // Combine and compute statistics
                var allLogs = recentLogs.Select(MapToDto).Concat(fileLogs).ToList();

                var now = DateTimeOffset.UtcNow;
                var statistics = new LogStatistics
                {
                    Total = allLogs.Count,
                    Errors = allLogs.Count(log => log.Level.Equals("Error", StringComparison.OrdinalIgnoreCase)),
                    Warnings = allLogs.Count(log => log.Level.Equals("Warning", StringComparison.OrdinalIgnoreCase)),
                    Info = allLogs.Count(log => log.Level.Equals("Information", StringComparison.OrdinalIgnoreCase)),
                    Debug = allLogs.Count(log => log.Level.Equals("Debug", StringComparison.OrdinalIgnoreCase)),
                    Critical = allLogs.Count(log => log.Level.Equals("Critical", StringComparison.OrdinalIgnoreCase)),
                    Today = allLogs.Count(log => log.Timestamp.UtcDateTime.Date == now.UtcDateTime.Date),
                    ThisWeek = allLogs.Count(log => log.Timestamp >= now.AddDays(-7)),
                    ThisMonth = allLogs.Count(log => log.Timestamp >= now.AddMonths(-1))
                };

                return statistics;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex, "Error getting quick stats");
                //return new LogStatistics(); // ✅ always return something
                throw new Exception("message", ex);
            }
        }




        // Parse log line
        private SystemLogDto? ParseLogLine(string line, LogSource source)
        {
            try
            {
                var parts = line.Split('[');
                if (parts.Length < 2) return null;

                var timestamp = parts[0].Trim();
                var levelEnd = parts[1].IndexOf(']');
                if (levelEnd == -1) return null;

                var level = parts[1][..levelEnd];
                var remaining = parts[1][(levelEnd + 1)..].Trim();

                // Parse message và metadata
                var messageEnd = remaining.IndexOf('(');
                string message, metadata = "";

                if (messageEnd > -1)
                {
                    message = remaining[..messageEnd].Trim();
                    metadata = remaining[(messageEnd + 1)..].Trim(')', ' ');
                }
                else
                {
                    message = remaining;
                }

                // Parse metadata
                string user = "Anonymous", ip = "Unknown";
                var fields = metadata.Split(',');
                foreach (var field in fields)
                {
                    var trimmed = field.Trim();
                    if (trimmed.StartsWith("User:")) user = trimmed[5..].Trim();
                    else if (trimmed.StartsWith("IP:")) ip = trimmed[3..].Trim();
                }

                return new SystemLogDto
                {
                    Timestamp = DateTimeOffset.Parse(timestamp),
                    Level = level,
                    Source = source.ToString(),
                    UserName = user,
                    Message = message,
                    IpAddress = ip
                };
            }
            catch
            {
                return null;
            }
        }

        // Map to DTO
        private SystemLogDto MapToDto(SystemLog log) => new()
        {
            Id = log.Id,
            Timestamp = log.Timestamp,
            Level = log.Level.ToString(),
            Source = log.Source.ToString(),
            UserId = log.UserId,
            UserName = log.UserName,
            Message = log.Message,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            RequestId = log.RequestId,
            Details = log.Details
        };

        // Dọn dẹp
        public void CleanupOldLogFiles(int retentionDays = 30)
        {
            var logDirectory = Path.Combine(_env.ContentRootPath, "Logs");
            if (!Directory.Exists(logDirectory)) return;

            var cutoffDate = DateTime.Now.AddDays(-retentionDays);

            foreach (var file in Directory.GetFiles(logDirectory, "*.log*"))
            {
                if (new FileInfo(file).LastWriteTime < cutoffDate)
                {
                    try { File.Delete(file); } catch { /* Ignore */ }
                }
            }
        }
    }
}

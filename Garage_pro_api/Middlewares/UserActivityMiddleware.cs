using System.Security.Claims;
using BusinessObject.SystemLogs;
using Services.LogServices;

namespace Garage_pro_api.Middlewares
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogService logService)
        {
            var user = context.User.Identity?.Name ?? "Anonymous";
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null;
            
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var agent = context.Request.Headers["User-Agent"].ToString();
            var path = context.Request.Path;
            var method = context.Request.Method;

            // Gọi pipeline kế tiếp và bắt trạng thái trả về
            var originalResponseBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            // Đọc lại status code sau khi xử lý request
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            var statusCode = context.Response.StatusCode;

            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                //  Người dùng chưa đăng nhập
                await logService.LogSecurityAsync(
                    $"Unauthorized access attempt to {method} {path}",
                    userId
                );
            }
            else if (statusCode == StatusCodes.Status403Forbidden)
            {
                //  Người dùng đăng nhập nhưng không có quyền
                await logService.LogSecurityAsync(
                    $"Forbidden access attempt by {user} to {method} {path}",
                    userId
                );
            }
            else if (statusCode >= 200 && statusCode < 300)
            {
                // Truy cập hợp lệ, chỉ log hoạt động người dùng
                var pathValue = path.Value ?? string.Empty;

                // Bỏ qua các endpoint thống kê
                var isStatsEndpoint =
                    pathValue.Contains("/api/ActivityLogs/stats", StringComparison.OrdinalIgnoreCase) ||
                    pathValue.Contains("/api/ActivityLogs/quick-stats", StringComparison.OrdinalIgnoreCase) ||
                    pathValue.Contains("/api/ActivityLogs/statistics", StringComparison.OrdinalIgnoreCase);

                if (!isStatsEndpoint)
                {
                    // Xác định hành động tương ứng với HTTP method
                    string userAction = method.ToUpper() switch
                    {
                        "GET" => $"viewed information at {pathValue}",
                        "POST" => $"created a new resource at {pathValue}",
                        "PUT" => $"updated resource at {pathValue}",
                        "PATCH" => $"partially updated resource at {pathValue}",
                        "DELETE" => $"deleted resource at {pathValue}",
                        _ => $"accessed {pathValue} using {method}"
                    };

                    // Định dạng thân thiện hiển thị trên giao diện
                    string logMessage = $"User {user ?? userId} {userAction}";

                    await logService.LogUserActivityAsync(
                        logMessage,
                        userId,
                        user
                    );
                }
            }
            else if (statusCode >= 400)
            {
                //  Lỗi client hoặc server
                await logService.LogSystemAsync(
                    $"Error {statusCode} when accessing {method} {path}",
                    LogLevel.Warning
                );
            }
        }
    }
}

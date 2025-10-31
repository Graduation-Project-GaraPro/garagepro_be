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
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var agent = context.Request.Headers["User-Agent"].ToString();
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // Ghi nhận nội dung phản hồi tạm thời
            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            // Sao chép lại nội dung response về output gốc
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            var statusCode = context.Response.StatusCode;

            // Nhóm các hành động log theo loại
            switch (statusCode)
            {
                case StatusCodes.Status401Unauthorized:
                    await logService.LogUserActivityAsync(
                        $"Unauthorized access attempt to {method} {path} from IP {ip}",
                        userId,user
                    );
                    break;

                case StatusCodes.Status403Forbidden:
                    await logService.LogSecurityAsync(
                        $"Forbidden access attempt by {user} ({userId}) to {method} {path} from IP {ip}",
                        userId
                    );
                    break;

                case StatusCodes.Status400BadRequest:
                    await logService.LogSystemAsync(
                        $"Client error {statusCode} on {method} {path} by {user} ({userId})",
                        LogLevel.Information
                    );
                    break;

                default:
                    if (statusCode >= 200 && statusCode < 300)
                    {
                        if (!IsStatsEndpoint(path))
                        {
                            var action = method.ToUpper() switch
                            {
                                "GET" => $"viewed information at {path}",
                                "POST" => $"created a new resource at {path}",
                                "PUT" => $"updated resource at {path}",
                                "PATCH" => $"partially updated resource at {path}",
                                "DELETE" => $"deleted resource at {path}",
                                _ => $"accessed {path} using {method}"
                            };

                            await logService.LogUserActivityAsync(
                                $"User {user} ({userId}) {action}",
                                userId,
                                user
                            );
                        }
                    }
                    else if (statusCode >= 400 && statusCode <500)
                    {
                        await logService.LogSystemAsync(
                            $"Unexpected error {statusCode} on {method} {path} by {user} ({userId})",
                            LogLevel.Information
                        );
                    }
                    else if (statusCode >= 500)
                    {
                        await logService.LogSystemAsync(
                            $"Server error {statusCode} on {method} {path} by {user} ({userId}) from {ip}",
                            LogLevel.Error
                        );
                    }
                    break;
                   
            }

           
            static bool IsStatsEndpoint(string path) =>
                path.Contains("/api/ActivityLogs/stats", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/api/ActivityLogs/quick-stats", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/api/ActivityLogs/statistics", StringComparison.OrdinalIgnoreCase);
        }
    }
}

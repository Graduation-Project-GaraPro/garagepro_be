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
            var userId = context.User.FindFirst("sub")?.Value ?? "N/A";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var agent = context.Request.Headers["User-Agent"].ToString();
            var path = context.Request.Path;

            // log access, không nhạy cảm
            await logService.LogUserActivityAsync(userId, user, $"Accessed {path}", ip, agent);

            await _next(context);
        }
    }
}

using BusinessObject.SystemLogs;
using Services.LogServices;

namespace Garage_pro_api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogService logService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await logService.LogErrorAsync(
                    ex,
                    $"Unhandled exception in {context.Request.Method} {context.Request.Path}"
                );

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
    }
}

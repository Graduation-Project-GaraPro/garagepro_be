using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Services.Authentication;
using Services.PolicyServices;

namespace Garage_pro_api.Middlewares
{
    public class SecurityPolicyMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityPolicyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DynamicAuthenticationService authService)
        {
            //    //Resolve scoped service trong scope request
            //    var securityPolicyService = context.RequestServices.GetRequiredService<ISecurityPolicyService>();

            //    if (context.User.Identity?.IsAuthenticated == true)
            //    {
            //        var user = await authService.GetUserAsync(context.User);
            //        if (user != null)
            //        {
            //            // Kiểm tra password expiry real-time
            //            if (await authService.IsPasswordExpiredAsync(user))
            //            {
            //                if (!context.Request.Path.StartsWithSegments("/api/auth/change-password") &&
            //                    !context.Request.Path.StartsWithSegments("/api/auth/logout"))
            //                {
            //                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
            //                    await context.Response.WriteAsJsonAsync(new
            //                    {
            //                        message = "Password has expired. Please change your password.",
            //                        requiresPasswordChange = true,
            //                        code = "PASSWORD_EXPIRED"
            //                    });
            //                    return;
            //                }
            //            }

            //            // Kiểm tra session timeout real-time
            //            await ValidateSessionTimeoutAsync(context, user, authService, securityPolicyService);
            //        }
            //    }

                await _next(context);
        }


    }

    // Extension method để dễ đăng ký middleware
    public static class SecurityPolicyMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityPolicyEnforcement(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityPolicyMiddleware>();
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Project02.Middleware;
public class AdminMiddleware
{
    private readonly RequestDelegate _next;

    public AdminMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        // Kiểm tra nếu URL bắt đầu bằng /admin
        if (path != null && path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
        {
            await context.SignOutAsync("UserScheme");
            // BỎ QUA login
            if (path.StartsWith("/admin/login", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var result = await context.AuthenticateAsync("AdminScheme");
            var user = result.Principal;

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                context.Response.Redirect("/admin/login");
                return;
            }

            if (!user.IsInRole("Admin"))
            {
                
                await context.SignOutAsync("AdminScheme");
                context.Response.Redirect("/admin/login?reason=forbidden");
                return;
            }
        }
        // Cho qua middleware tiếp theo
        await _next(context);
    }
}

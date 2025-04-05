using Microsoft.AspNetCore.Authentication;

namespace RazorInMemoryDemo.Middleware
{
    public class UserBanCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public UserBanCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the user is banned (set by ActiveUserHandler)
            var isBanned = context.Session.GetString("UserBanned");
            
            if (isBanned == "true")
            {
                // Clear the ban flag from session
                context.Session.Remove("UserBanned");
                
                // Clear all session data
                context.Session.Clear();
                
                // Sign out
                await context.SignOutAsync("CookieAuth");
                
                // Redirect to login with a message
                context.Response.Redirect("/Account/Login?banned=true");
                return;
            }
            
            await _next(context);
        }
    }
    
    // Extension method to add the middleware to the application pipeline
    public static class UserBanCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserBanCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserBanCheckMiddleware>();
        }
    }
} 
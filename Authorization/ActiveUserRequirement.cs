using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo
{
    // Requirement that a user must be active (not banned)
    public class ActiveUserRequirement : IAuthorizationRequirement
    {
        // No additional properties needed
    }

    // Handler to verify the requirement
    public class ActiveUserHandler : AuthorizationHandler<ActiveUserRequirement>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActiveUserHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            ActiveUserRequirement requirement)
        {
            // If no user is authenticated, skip this check
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            // Get the username from claims
            var username = context.User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Task.CompletedTask;
            }

            // Get the user from database to check if they're active
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            
            // If user doesn't exist in database, they can't proceed
            if (user == null)
            {
                // Force logout on next request
                RedirectToLogout();
                return Task.CompletedTask;
            }

            // Check if the user is active
            if (user.IsActive)
            {
                context.Succeed(requirement);
            }
            else
            {
                // User is banned, force logout
                RedirectToLogout();
            }

            return Task.CompletedTask;
        }

        private void RedirectToLogout()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                // Set a session flag that will be checked on the next request
                // to trigger a logout and redirect to login page
                _httpContextAccessor.HttpContext.Session.SetString("UserBanned", "true");
            }
        }
    }
} 
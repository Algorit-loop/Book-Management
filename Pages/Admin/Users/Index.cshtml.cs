using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo.Pages.Admin.Users
{
    [Authorize(Policy = "AdminPolicy")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<User> Users { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            if (_context.Users != null)
            {
                Users = await _context.Users.ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostBanAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Don't allow banning admin users (including yourself)
            if (user.Role == "Admin")
            {
                StatusMessage = "Admin users cannot be banned.";
                return RedirectToPage();
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
            
            StatusMessage = $"User '{user.Username}' has been banned.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnbanAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();
            
            StatusMessage = $"User '{user.Username}' has been unbanned.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Don't allow deleting admin users (including yourself)
            if (user.Role == "Admin")
            {
                StatusMessage = "Admin users cannot be deleted.";
                return RedirectToPage();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            StatusMessage = $"User '{user.Username}' has been deleted.";
            return RedirectToPage();
        }
    }
} 
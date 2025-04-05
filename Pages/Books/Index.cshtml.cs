using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo.Pages.Books
{
    [Authorize(Policy = "UserPolicy")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IndexModel(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IList<Book> Books { get; set; } = default!;
        public string? CurrentUsername { get; set; }
        public string? CurrentRole { get; set; }
        public bool IsAdmin { get; set; }

        public async Task OnGetAsync()
        {
            if (_context.Books != null)
            {
                Books = await _context.Books.ToListAsync();
            }

            // Get user info from session
            CurrentUsername = HttpContext.Session.GetString("Username");
            CurrentRole = HttpContext.Session.GetString("Role");
            IsAdmin = CurrentRole == "Admin";
        }
    }
} 
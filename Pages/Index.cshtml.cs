using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public string? CurrentUsername { get; set; }
    public string? CurrentRole { get; set; }
    public bool IsLoggedIn { get; set; }
    public IList<Book> RecentBooks { get; set; } = default!;

    public async Task OnGetAsync()
    {
        // Get user info from session
        CurrentUsername = HttpContext.Session.GetString("Username");
        CurrentRole = HttpContext.Session.GetString("Role");
        IsLoggedIn = !string.IsNullOrEmpty(CurrentUsername);

        // Get recent books
        if (_context.Books != null)
        {
            RecentBooks = await _context.Books.Take(3).ToListAsync();
        }
    }
}

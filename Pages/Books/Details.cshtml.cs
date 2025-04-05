using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo.Pages.Books
{
    [Authorize(Policy = "UserPolicy")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Book Book { get; set; } = default!;
        public string? CurrentRole { get; set; }
        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            
            Book = book;
            CurrentRole = HttpContext.Session.GetString("Role");
            IsAdmin = CurrentRole == "Admin";
            
            return Page();
        }
    }
} 
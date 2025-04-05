using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo.Pages.Books
{
    [Authorize(Policy = "AdminPolicy")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Book Book { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.Books == null || Book == null)
            {
                return Page();
            }

            _context.Books.Add(Book);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorInMemoryDemo.Models;

namespace RazorInMemoryDemo.Pages.Books
{
    [Authorize(Policy = "AdminPolicy")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Book Book { get; set; } = default!;

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
            else 
            {
                Book = book;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                Book = book;
                _context.Books.Remove(Book);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
} 
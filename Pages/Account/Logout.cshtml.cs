using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorInMemoryDemo.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            
            // Clear session
            HttpContext.Session.Clear();
            
            return RedirectToPage("/Index");
        }
    }
} 
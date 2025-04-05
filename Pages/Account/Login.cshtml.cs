using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorInMemoryDemo.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace RazorInMemoryDemo.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginModel(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string ReturnUrl { get; set; } = "";

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => 
                    u.Username == Input.Username && 
                    u.Password == Input.Password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("Role", user.Role)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Store user info in session
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }
    }
} 
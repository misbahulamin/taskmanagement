using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TaskManagement.Models;
using TaskManagement.Services;

namespace TaskManagement.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public LoginViewModel LoginInput { get; set; } = new LoginViewModel();

        public string ErrorMessage { get; set; } = string.Empty;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // If user is already logged in, redirect to home page
            if (UserSessionHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Tasks/Index");
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Tasks/Index");

            if (ModelState.IsValid)
            {
                var user = await _authService.AuthenticateAsync(
                    LoginInput.UsernameOrEmail,
                    LoginInput.Password);

                if (user != null)
                {
                    _logger.LogInformation("User {Email} logged in at {Time}",
                        user.Email, DateTime.UtcNow);

                    // Store user info in session using our helper
                    UserSessionHelper.SetUserSession(HttpContext, user);

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    ErrorMessage = "Invalid username/email or password.";
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
} 
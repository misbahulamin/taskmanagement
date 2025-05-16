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

        public void OnGet()
        {
            // If user is already logged in, redirect to home page
            if (HttpContext.Session.GetString("CurrentUser") != null)
            {
                Response.Redirect("/");
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _authService.AuthenticateAsync(
                    LoginInput.UsernameOrEmail,
                    LoginInput.Password);

                if (user != null)
                {
                    _logger.LogInformation("User {Email} logged in at {Time}",
                        user.Email, DateTime.UtcNow);

                    // Store user info in session
                    var sessionUser = new
                    {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.FirstName,
                        user.LastName
                    };

                    HttpContext.Session.SetString("CurrentUser", 
                        JsonSerializer.Serialize(sessionUser));

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
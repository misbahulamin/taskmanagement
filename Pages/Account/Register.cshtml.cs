using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TaskManagement.Models;
using TaskManagement.Services;

namespace TaskManagement.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public RegisterViewModel RegisterInput { get; set; } = new RegisterViewModel();

        public string ErrorMessage { get; set; } = string.Empty;

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // If user is already logged in, redirect to tasks page
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
                try
                {
                    var user = await _authService.RegisterUserAsync(
                        RegisterInput.Username,
                        RegisterInput.Email,
                        RegisterInput.Password,
                        RegisterInput.FirstName,
                        RegisterInput.LastName);

                    _logger.LogInformation("User {Email} registered at {Time}",
                        user.Email, DateTime.UtcNow);

                    // Automatically log in the user after registration
                    UserSessionHelper.SetUserSession(HttpContext, user);

                    return LocalRedirect(returnUrl);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    ErrorMessage = ex.Message;
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
} 
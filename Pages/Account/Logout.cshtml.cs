using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskManagement.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Clear the session
            HttpContext.Session.Clear();
            
            _logger.LogInformation("User logged out at {Time}", DateTime.UtcNow);
            
            return RedirectToPage("/Index");
        }
    }
} 
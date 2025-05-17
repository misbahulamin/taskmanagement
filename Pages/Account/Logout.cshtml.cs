using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagement.Services;

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
            // If it's a GET request, just show the logout page
            // The actual logout will happen in the POST request
            return Page();
        }
        
        public IActionResult OnPost()
        {
            // Get user info for logging before clearing session
            var userSession = UserSessionHelper.GetUserSession(HttpContext);
            string userIdentifier = userSession?.Email ?? userSession?.Username ?? "User";
            
            // Clear the user session
            UserSessionHelper.ClearUserSession(HttpContext);
            
            // Clear all session data
            HttpContext.Session.Clear();
            
            // Expire the session cookie
            if (HttpContext.Request.Cookies.Count > 0)
            {
                var siteCookies = HttpContext.Request.Cookies.Keys;
                foreach (var cookieName in siteCookies)
                {
                    HttpContext.Response.Cookies.Delete(cookieName);
                }
            }
            
            _logger.LogInformation("{User} logged out at {Time}", userIdentifier, DateTime.UtcNow);
            
            // Redirect to the home page
            return RedirectToPage("/Index");
        }
    }
} 
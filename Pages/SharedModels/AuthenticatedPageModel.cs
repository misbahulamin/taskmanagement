using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagement.Services;

namespace TaskManagement.Pages.SharedModels
{
    public abstract class AuthenticatedPageModel : PageModel
    {
        [ViewData]
        public string Title { get; set; } = string.Empty;
        
        [ViewData]
        public bool IsAuthenticated { get; set; }
        
        [ViewData]
        public string? CurrentUsername { get; set; }
        
        [ViewData]
        public string? CurrentUserFullName { get; set; }
        
        public int UserId { get; private set; }

        public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
        {
            var userSession = UserSessionHelper.GetUserSession(HttpContext);
            
            IsAuthenticated = userSession != null;
            
            if (IsAuthenticated && userSession != null)
            {
                CurrentUsername = userSession.Username;
                CurrentUserFullName = !string.IsNullOrEmpty(userSession.FirstName) && !string.IsNullOrEmpty(userSession.LastName)
                    ? $"{userSession.FirstName} {userSession.LastName}"
                    : userSession.Username;
                
                UserId = userSession.Id;
            }
            else
            {
                // Redirect to login page if not authenticated
                context.Result = new RedirectToPageResult("/Account/Login");
                return;
            }
            
            base.OnPageHandlerExecuting(context);
        }
    }
} 
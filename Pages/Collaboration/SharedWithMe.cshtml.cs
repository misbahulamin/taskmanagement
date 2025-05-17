using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManagement.Models;
using TaskManagement.Services;

namespace TaskManagement.Pages.Collaboration
{
    public class SharedWithMeModel : PageModel
    {
        private readonly ICollaborationService _collaborationService;
        private readonly ILogger<SharedWithMeModel> _logger;

        public List<UserTask> SharedTasks { get; set; } = new List<UserTask>();
        
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public SharedWithMeModel(
            ICollaborationService collaborationService, 
            ILogger<SharedWithMeModel> logger)
        {
            _collaborationService = collaborationService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            if (!UserSessionHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            var userId = UserSessionHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }
            
            // Get tasks shared with the user
            SharedTasks = await _collaborationService.GetSharedTasksAsync(userId.Value);
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAcceptTaskAsync(int taskId)
        {
            // Additional functionality could be added here if needed
            await Task.CompletedTask; // Add await to avoid warning
            return RedirectToPage();
        }
    }
} 
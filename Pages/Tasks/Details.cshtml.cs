using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Pages.SharedModels;
using TaskManagement.Services;
using TaskManagement.Utils;

namespace TaskManagement.Pages.Tasks
{
    public class DetailsModel : AuthenticatedPageModel
    {
        private readonly ITaskService _taskService;
        private readonly ICollaborationService _collaborationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetailsModel> _logger;

        public UserTask? Task { get; set; }
        public List<TaskCollaborator> Collaborators { get; set; } = new List<TaskCollaborator>();
        public bool CanEdit { get; set; }
        
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public DetailsModel(
            ITaskService taskService,
            ICollaborationService collaborationService,
            ApplicationDbContext context,
            ILogger<DetailsModel> logger)
        {
            _taskService = taskService;
            _collaborationService = collaborationService;
            _context = context;
            _logger = logger;
            Title = "Task Details";
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // First check if this is the user's own task
            Task = await _taskService.GetTaskByIdAsync(id.Value, UserId);
            
            // If not found, check if it's a shared task
            if (Task == null)
            {
                if (await _collaborationService.CanAccessTaskAsync(id.Value, UserId))
                {
                    // Get the task regardless of owner
                    Task = await _context.Tasks
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.Id == id.Value);
                    
                    // Check edit permissions
                    CanEdit = await _collaborationService.CanEditTaskAsync(id.Value, UserId);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                // User is the owner, so they can edit
                CanEdit = true;
                
                // Get collaborators if the user is the owner
                Collaborators = await _collaborationService.GetTaskCollaboratorsAsync(id.Value);
            }

            return Page();
        }
        
        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            // First check if user can edit this task
            if (!await _collaborationService.CanEditTaskAsync(id, UserId))
            {
                StatusMessage = "Error: You don't have permission to edit this task.";
                return RedirectToPage(new { id });
            }
            
            await _taskService.MarkTaskAsCompletedAsync(id, UserId);
            StatusMessage = "Task marked as completed.";
            return RedirectToPage(new { id });
        }
        
        public async Task<IActionResult> OnPostUncompleteAsync(int id)
        {
            // First check if user can edit this task
            if (!await _collaborationService.CanEditTaskAsync(id, UserId))
            {
                StatusMessage = "Error: You don't have permission to edit this task.";
                return RedirectToPage(new { id });
            }
            
            await _taskService.MarkTaskAsNotCompletedAsync(id, UserId);
            StatusMessage = "Task marked as not completed.";
            return RedirectToPage(new { id });
        }
    }
} 
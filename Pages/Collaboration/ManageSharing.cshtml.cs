using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagement.Models;
using TaskManagement.Services;

namespace TaskManagement.Pages.Collaboration
{
    public class ManageSharingModel : PageModel
    {
        private readonly ICollaborationService _collaborationService;
        private readonly ITaskService _taskService;
        private readonly ILogger<ManageSharingModel> _logger;

        public UserTask? Task { get; set; }
        public List<TaskCollaborator> Collaborators { get; set; } = new List<TaskCollaborator>();
        
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;
        
        [BindProperty]
        public int TaskId { get; set; }
        
        [BindProperty]
        public string CollaboratorEmail { get; set; } = string.Empty;
        
        [BindProperty]
        public CollaboratorRole Role { get; set; } = CollaboratorRole.Viewer;
        
        [BindProperty]
        public bool CanEdit { get; set; } = false;
        
        public SelectList RoleOptions { get; set; }

        public ManageSharingModel(
            ICollaborationService collaborationService,
            ITaskService taskService,
            ILogger<ManageSharingModel> logger)
        {
            _collaborationService = collaborationService;
            _taskService = taskService;
            _logger = logger;
            
            // Initialize the role options for the select list
            RoleOptions = new SelectList(
                Enum.GetValues(typeof(CollaboratorRole))
                    .Cast<CollaboratorRole>()
                    .Select(r => new { Id = (int)r, Name = r.ToString() }),
                "Id", "Name");
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Check if user is authenticated
            if (!UserSessionHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }

            if (id == null)
            {
                return NotFound();
            }
            
            TaskId = id.Value;
            
            var userId = UserSessionHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }
            
            // Get the task
            Task = await _taskService.GetTaskByIdAsync(TaskId, userId.Value);
            
            if (Task == null)
            {
                return NotFound();
            }
            
            // Get collaborators for this task
            Collaborators = await _collaborationService.GetTaskCollaboratorsAsync(TaskId);
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAddCollaboratorAsync()
        {
            if (!UserSessionHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }
            
            if (string.IsNullOrWhiteSpace(CollaboratorEmail))
            {
                StatusMessage = "Error: You must provide an email or username to share with.";
                return RedirectToPage(new { id = TaskId });
            }
            
            var userId = UserSessionHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }
            
            try
            {
                // Find the user to share with
                var collaborator = await _collaborationService.FindUserForSharingAsync(CollaboratorEmail);
                
                if (collaborator == null)
                {
                    StatusMessage = "Error: User not found. Please check the email or username.";
                    return RedirectToPage(new { id = TaskId });
                }
                
                if (collaborator.Id == userId)
                {
                    StatusMessage = "Error: You cannot share a task with yourself.";
                    return RedirectToPage(new { id = TaskId });
                }
                
                // Add collaborator to the task
                var result = await _collaborationService.AddCollaboratorAsync(
                    TaskId, userId.Value, collaborator.Id, Role, CanEdit);
                
                StatusMessage = $"Task shared successfully with {collaborator.Username}.";
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing task {TaskId}", TaskId);
                StatusMessage = "Error: An unexpected error occurred while sharing the task.";
            }
            
            return RedirectToPage(new { id = TaskId });
        }
        
        public async Task<IActionResult> OnPostRemoveCollaboratorAsync(int collaboratorId)
        {
            if (!UserSessionHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }
            
            var userId = UserSessionHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }
            
            try
            {
                var result = await _collaborationService.RemoveCollaboratorAsync(
                    TaskId, userId.Value, collaboratorId);
                
                if (result)
                {
                    StatusMessage = "Collaborator removed successfully.";
                }
                else
                {
                    StatusMessage = "Error: Failed to remove collaborator.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing collaborator {CollaboratorId} from task {TaskId}", 
                    collaboratorId, TaskId);
                StatusMessage = "Error: An unexpected error occurred while removing the collaborator.";
            }
            
            return RedirectToPage(new { id = TaskId });
        }
        
        public async Task<IActionResult> OnPostUpdateCollaboratorAsync(int collaboratorId, CollaboratorRole role, bool canEdit)
        {
            if (!UserSessionHelper.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Account/Login");
            }
            
            var userId = UserSessionHelper.GetUserId(HttpContext);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }
            
            try
            {
                var result = await _collaborationService.UpdateCollaboratorAsync(
                    TaskId, userId.Value, collaboratorId, role, canEdit);
                
                if (result)
                {
                    StatusMessage = "Collaborator permissions updated successfully.";
                }
                else
                {
                    StatusMessage = "Error: Failed to update collaborator permissions.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating collaborator {CollaboratorId} for task {TaskId}", 
                    collaboratorId, TaskId);
                StatusMessage = "Error: An unexpected error occurred while updating the collaborator.";
            }
            
            return RedirectToPage(new { id = TaskId });
        }
    }
} 
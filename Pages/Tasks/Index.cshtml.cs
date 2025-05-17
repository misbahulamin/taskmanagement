using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagement.Models;
using TaskManagement.Pages.SharedModels;
using TaskManagement.Services;

namespace TaskManagement.Pages.Tasks
{
    public class IndexModel : AuthenticatedPageModel
    {
        private readonly ITaskService _taskService;

        public List<UserTask> Tasks { get; set; } = new List<UserTask>();
        
        [BindProperty(SupportsGet = true)]
        public TaskFilterViewModel Filter { get; set; } = new TaskFilterViewModel();
        
        public IndexModel(ITaskService taskService)
        {
            _taskService = taskService;
            Title = "My Tasks";
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            // Load categories for filter
            var categories = await _taskService.GetUserCategoriesAsync(UserId);
            Filter.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Categories" }
            };
            Filter.Categories.AddRange(categories.Select(c => new SelectListItem { Value = c, Text = c }));
            
            // Apply filters
            if (!string.IsNullOrEmpty(Filter.SearchTerm))
            {
                Tasks = await _taskService.SearchUserTasksAsync(UserId, Filter.SearchTerm);
            }
            else if (Filter.Priority.HasValue)
            {
                Tasks = await _taskService.GetUserTasksByPriorityAsync(UserId, Filter.Priority.Value);
            }
            else if (!string.IsNullOrEmpty(Filter.Category))
            {
                Tasks = await _taskService.GetUserTasksByCategoryAsync(UserId, Filter.Category);
            }
            else if (Filter.IsCompleted.HasValue)
            {
                Tasks = await _taskService.GetUserTasksAsync(UserId, Filter.IsCompleted.Value);
            }
            else
            {
                Tasks = await _taskService.GetUserTasksAsync(UserId);
            }
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            await _taskService.MarkTaskAsCompletedAsync(id, UserId);
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostUncompleteAsync(int id)
        {
            await _taskService.MarkTaskAsNotCompletedAsync(id, UserId);
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _taskService.DeleteTaskAsync(id, UserId);
            return RedirectToPage();
        }
    }
} 
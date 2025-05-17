using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagement.Models;
using TaskManagement.Pages.SharedModels;
using TaskManagement.Services;

namespace TaskManagement.Pages.Tasks
{
    public class CreateModel : AuthenticatedPageModel
    {
        private readonly ITaskService _taskService;

        [BindProperty]
        public UserTask Task { get; set; } = new UserTask();

        public List<SelectListItem> PriorityOptions { get; set; } = new List<SelectListItem>();

        public CreateModel(ITaskService taskService)
        {
            _taskService = taskService;
            Title = "Create New Task";
            
            // Initialize the priority options
            PriorityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = Models.Priority.Low.ToString(), Text = "Low" },
                new SelectListItem { Value = Models.Priority.Medium.ToString(), Text = "Medium", Selected = true },
                new SelectListItem { Value = Models.Priority.High.ToString(), Text = "High" },
                new SelectListItem { Value = Models.Priority.Urgent.ToString(), Text = "Urgent" }
            };
        }

        public IActionResult OnGet()
        {
            // Set default values for the new task
            Task.CreatedAt = DateTime.Now;
            Task.Priority = Models.Priority.Medium;
            Task.UserId = UserId;
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Set the user ID and creation date
            Task.UserId = UserId;
            Task.CreatedAt = DateTime.Now;
            
            await _taskService.CreateTaskAsync(Task);
            
            // Redirect to the tasks list
            return RedirectToPage("./Index");
        }
    }
} 
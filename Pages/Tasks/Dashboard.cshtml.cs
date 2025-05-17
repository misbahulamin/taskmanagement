using Microsoft.AspNetCore.Mvc;
using TaskManagement.Models;
using TaskManagement.Pages.SharedModels;
using TaskManagement.Services;

namespace TaskManagement.Pages.Tasks
{
    public class DashboardModel : AuthenticatedPageModel
    {
        private readonly ITaskService _taskService;

        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int ActiveTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        
        public Dictionary<Priority, int> TasksByPriority { get; set; } = new Dictionary<Priority, int>();
        public Dictionary<string, int> TasksByCategory { get; set; } = new Dictionary<string, int>();
        
        public List<UserTask> RecentlyCompletedTasks { get; set; } = new List<UserTask>();
        public List<UserTask> TasksDueSoon { get; set; } = new List<UserTask>();

        public DashboardModel(ITaskService taskService)
        {
            _taskService = taskService;
            Title = "Task Analytics Dashboard";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get all user tasks
            var allTasks = await _taskService.GetUserTasksAsync(UserId);
            
            // Calculate basic statistics
            TotalTasks = allTasks.Count;
            CompletedTasks = allTasks.Count(t => t.IsCompleted);
            ActiveTasks = allTasks.Count(t => !t.IsCompleted);
            HighPriorityTasks = allTasks.Count(t => t.Priority == Priority.High || t.Priority == Priority.Urgent);
            
            // Tasks by priority
            TasksByPriority = allTasks
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Ensure all priority levels are represented
            foreach (Priority priority in Enum.GetValues(typeof(Priority)))
            {
                if (!TasksByPriority.ContainsKey(priority))
                {
                    TasksByPriority[priority] = 0;
                }
            }
            
            // Order by priority level
            TasksByPriority = TasksByPriority
                .OrderBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            
            // Tasks by category
            var tasksByCategory = allTasks
                .GroupBy(t => t.Category ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Order by count descending
            TasksByCategory = tasksByCategory
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            
            // Recently completed tasks (last 5)
            RecentlyCompletedTasks = allTasks
                .Where(t => t.IsCompleted && t.CompletedAt.HasValue)
                .OrderByDescending(t => t.CompletedAt)
                .Take(5)
                .ToList();
            
            // Tasks due soon (next 7 days)
            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);
            
            TasksDueSoon = allTasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date >= today && t.DueDate.Value.Date <= nextWeek)
                .OrderBy(t => t.DueDate)
                .ToList();
            
            return Page();
        }
    }
} 
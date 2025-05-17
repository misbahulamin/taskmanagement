using TaskManagement.Models;

namespace TaskManagement.Services
{
    public class TaskAnalyticsService : ITaskAnalyticsService
    {
        private readonly ITaskService _taskService;

        public TaskAnalyticsService(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public async Task<Dictionary<string, int>> GetTaskCompletionTrendAsync(int userId, int days)
        {
            var allTasks = await _taskService.GetUserTasksAsync(userId);
            var result = new Dictionary<string, int>();
            
            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Today.AddDays(-i);
                var dateString = date.ToString("MMM d");
                
                var completedCount = allTasks.Count(t => 
                    t.CompletedAt.HasValue && 
                    t.CompletedAt.Value.Date == date);
                
                result.Add(dateString, completedCount);
            }
            
            // Reverse to get chronological order
            return result.Reverse().ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<double> GetTaskCompletionRateAsync(int userId, int days)
        {
            var allTasks = await _taskService.GetUserTasksAsync(userId);
            var startDate = DateTime.Today.AddDays(-days);
            
            var tasksInPeriod = allTasks.Where(t => 
                t.CreatedAt.Date >= startDate || 
                (t.CompletedAt.HasValue && t.CompletedAt.Value.Date >= startDate)).ToList();
            
            if (!tasksInPeriod.Any())
                return 0;
            
            var completedCount = tasksInPeriod.Count(t => t.IsCompleted);
            return Math.Round((double)completedCount / tasksInPeriod.Count * 100, 1);
        }

        public async Task<Dictionary<string, int>> GetTaskCountByDueDateAsync(int userId)
        {
            var allTasks = await _taskService.GetUserTasksAsync(userId);
            var result = new Dictionary<string, int>
            {
                { "Overdue", 0 },
                { "Today", 0 },
                { "Tomorrow", 0 },
                { "This Week", 0 },
                { "Next Week", 0 },
                { "Later", 0 },
                { "No Due Date", 0 }
            };
            
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var nextWeek = today.AddDays(7);
            var twoWeeks = today.AddDays(14);
            
            foreach (var task in allTasks.Where(t => !t.IsCompleted))
            {
                if (!task.DueDate.HasValue)
                {
                    result["No Due Date"]++;
                }
                else if (task.DueDate.Value.Date < today)
                {
                    result["Overdue"]++;
                }
                else if (task.DueDate.Value.Date == today)
                {
                    result["Today"]++;
                }
                else if (task.DueDate.Value.Date == tomorrow)
                {
                    result["Tomorrow"]++;
                }
                else if (task.DueDate.Value.Date > tomorrow && task.DueDate.Value.Date <= nextWeek)
                {
                    result["This Week"]++;
                }
                else if (task.DueDate.Value.Date > nextWeek && task.DueDate.Value.Date <= twoWeeks)
                {
                    result["Next Week"]++;
                }
                else
                {
                    result["Later"]++;
                }
            }
            
            return result;
        }

        public async Task<int> GetTasksCreatedTodayAsync(int userId)
        {
            var allTasks = await _taskService.GetUserTasksAsync(userId);
            var today = DateTime.Today;
            
            return allTasks.Count(t => t.CreatedAt.Date == today);
        }

        public async Task<int> GetTasksCompletedTodayAsync(int userId)
        {
            var allTasks = await _taskService.GetUserTasksAsync(userId);
            var today = DateTime.Today;
            
            return allTasks.Count(t => t.IsCompleted && t.CompletedAt.HasValue && t.CompletedAt.Value.Date == today);
        }
    }
} 
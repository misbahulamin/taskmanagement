using TaskManagement.Models;

namespace TaskManagement.Services
{
    public interface ITaskService
    {
        Task<List<UserTask>> GetUserTasksAsync(int userId);
        Task<List<UserTask>> GetUserTasksAsync(int userId, bool isCompleted);
        Task<List<UserTask>> GetUserTasksByPriorityAsync(int userId, Priority priority);
        Task<List<UserTask>> GetUserTasksByCategoryAsync(int userId, string category);
        Task<List<UserTask>> SearchUserTasksAsync(int userId, string searchTerm);
        Task<UserTask?> GetTaskByIdAsync(int taskId, int userId);
        Task<UserTask> CreateTaskAsync(UserTask task);
        Task<UserTask?> UpdateTaskAsync(UserTask task);
        Task<bool> DeleteTaskAsync(int taskId, int userId);
        Task<bool> MarkTaskAsCompletedAsync(int taskId, int userId);
        Task<bool> MarkTaskAsNotCompletedAsync(int taskId, int userId);
        Task<List<string>> GetUserCategoriesAsync(int userId);
    }
} 
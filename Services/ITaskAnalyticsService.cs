namespace TaskManagement.Services
{
    public interface ITaskAnalyticsService
    {
        Task<Dictionary<string, int>> GetTaskCompletionTrendAsync(int userId, int days);
        Task<double> GetTaskCompletionRateAsync(int userId, int days);
        Task<Dictionary<string, int>> GetTaskCountByDueDateAsync(int userId);
        Task<int> GetTasksCreatedTodayAsync(int userId);
        Task<int> GetTasksCompletedTodayAsync(int userId);
    }
} 
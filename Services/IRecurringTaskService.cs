using TaskManagement.Models;

namespace TaskManagement.Services
{
    public interface IRecurringTaskService
    {
        Task<RecurrencePattern> CreateRecurrencePatternAsync(RecurrencePattern pattern);
        Task<bool> UpdateRecurrencePatternAsync(RecurrencePattern pattern);
        Task<bool> DeleteRecurrencePatternAsync(int patternId);
        Task<List<UserTask>> GeneratePendingRecurringTasksAsync();
        Task<RecurrencePattern?> GetRecurrencePatternAsync(int taskId);
    }
} 
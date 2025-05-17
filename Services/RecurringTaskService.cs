using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public class RecurringTaskService : IRecurringTaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskService _taskService;

        public RecurringTaskService(ApplicationDbContext context, ITaskService taskService)
        {
            _context = context;
            _taskService = taskService;
        }

        public async Task<RecurrencePattern> CreateRecurrencePatternAsync(RecurrencePattern pattern)
        {
            // Ensure task exists
            var task = await _context.Tasks.FindAsync(pattern.TaskId);
            if (task == null)
            {
                throw new ArgumentException("Task not found");
            }

            // Mark task as recurring
            task.IsRecurring = true;
            
            // Add pattern to database
            await _context.RecurrencePatterns.AddAsync(pattern);
            await _context.SaveChangesAsync();
            
            return pattern;
        }

        public async Task<bool> UpdateRecurrencePatternAsync(RecurrencePattern pattern)
        {
            var existingPattern = await _context.RecurrencePatterns
                .FirstOrDefaultAsync(p => p.Id == pattern.Id);

            if (existingPattern == null)
            {
                return false;
            }

            // Update pattern properties
            existingPattern.Type = pattern.Type;
            existingPattern.Interval = pattern.Interval;
            existingPattern.DaysOfWeek = pattern.DaysOfWeek;
            existingPattern.DayOfMonth = pattern.DayOfMonth;
            existingPattern.WeekOfMonth = pattern.WeekOfMonth;
            existingPattern.StartDate = pattern.StartDate;
            existingPattern.EndDate = pattern.EndDate;
            existingPattern.Occurrences = pattern.Occurrences;

            _context.RecurrencePatterns.Update(existingPattern);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> DeleteRecurrencePatternAsync(int patternId)
        {
            var pattern = await _context.RecurrencePatterns
                .Include(p => p.Task)
                .FirstOrDefaultAsync(p => p.Id == patternId);

            if (pattern == null)
            {
                return false;
            }

            // Remove recurring flag from task
            if (pattern.Task != null)
            {
                pattern.Task.IsRecurring = false;
                _context.Tasks.Update(pattern.Task);
            }

            _context.RecurrencePatterns.Remove(pattern);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<List<UserTask>> GeneratePendingRecurringTasksAsync()
        {
            var today = DateTime.Today;
            var generatedTasks = new List<UserTask>();
            
            // Get all active recurrence patterns
            var patterns = await _context.RecurrencePatterns
                .Include(p => p.Task)
                .Where(p => p.Task != null && 
                           (p.EndDate == null || p.EndDate >= today))
                .ToListAsync();
            
            foreach (var pattern in patterns)
            {
                // Skip if original task is null
                if (pattern.Task == null) continue;
                
                // Determine the next occurrence date
                var lastGenerated = pattern.LastGenerated ?? pattern.StartDate.AddDays(-1);
                var nextDate = GetNextOccurrenceDate(pattern, lastGenerated);
                
                // If next date is in the future, skip
                if (nextDate > today) continue;
                
                // Check if we've reached the occurrence limit
                if (pattern.Occurrences.HasValue)
                {
                    var generatedCount = await _context.Tasks
                        .CountAsync(t => t.ParentTaskId == pattern.TaskId);
                    
                    if (generatedCount >= pattern.Occurrences.Value)
                    {
                        continue;
                    }
                }
                
                // Create the new recurring task instance
                var newTask = CreateTaskFromTemplate(pattern.Task, nextDate);
                newTask.ParentTaskId = pattern.TaskId;
                
                await _context.Tasks.AddAsync(newTask);
                generatedTasks.Add(newTask);
                
                // Update last generated date
                pattern.LastGenerated = nextDate;
                _context.RecurrencePatterns.Update(pattern);
            }
            
            await _context.SaveChangesAsync();
            return generatedTasks;
        }
        
        public async Task<RecurrencePattern?> GetRecurrencePatternAsync(int taskId)
        {
            return await _context.RecurrencePatterns
                .FirstOrDefaultAsync(p => p.TaskId == taskId);
        }
        
        private DateTime GetNextOccurrenceDate(RecurrencePattern pattern, DateTime lastDate)
        {
            DateTime nextDate;
            
            switch (pattern.Type)
            {
                case RecurrenceType.Daily:
                    nextDate = lastDate.AddDays(pattern.Interval);
                    break;
                    
                case RecurrenceType.Weekly:
                    // Find the next matching day of week
                    nextDate = CalculateNextWeeklyDate(pattern, lastDate);
                    break;
                    
                case RecurrenceType.Monthly:
                    // Find the next matching day in the next month
                    nextDate = CalculateNextMonthlyDate(pattern, lastDate);
                    break;
                    
                case RecurrenceType.Yearly:
                    nextDate = lastDate.AddYears(pattern.Interval);
                    break;
                    
                default:
                    nextDate = lastDate.AddDays(1);
                    break;
            }
            
            return nextDate;
        }
        
        private DateTime CalculateNextWeeklyDate(RecurrencePattern pattern, DateTime lastDate)
        {
            if (!pattern.DaysOfWeek.HasValue || pattern.DaysOfWeek.Value == 0)
            {
                // If no days specified, just add the interval in weeks
                return lastDate.AddDays(7 * pattern.Interval);
            }
            
            var daysOfWeek = pattern.DaysOfWeek.Value;
            var nextDate = lastDate.AddDays(1);
            
            // Check each day until we find a match
            while (true)
            {
                // Convert day of week to bit position (Sunday = 1, Monday = 2, etc.)
                int dayBit = 1 << ((int)nextDate.DayOfWeek);
                
                if ((daysOfWeek & dayBit) != 0)
                {
                    // If this day matches, return it
                    return nextDate;
                }
                
                nextDate = nextDate.AddDays(1);
                
                // If we've gone through a full week cycle, add the interval
                if (nextDate.Subtract(lastDate).Days > 7 * pattern.Interval)
                {
                    // Reset to start of next interval
                    nextDate = lastDate.AddDays(7 * pattern.Interval);
                }
            }
        }
        
        private DateTime CalculateNextMonthlyDate(RecurrencePattern pattern, DateTime lastDate)
        {
            if (pattern.DayOfMonth.HasValue)
            {
                // Monthly by day of month (e.g., 15th of each month)
                var day = Math.Min(pattern.DayOfMonth.Value, DateTime.DaysInMonth(lastDate.Year, lastDate.Month));
                var nextMonth = lastDate.AddMonths(pattern.Interval);
                return new DateTime(nextMonth.Year, nextMonth.Month, day);
            }
            else if (pattern.WeekOfMonth.HasValue)
            {
                // Monthly by position (e.g., 2nd Tuesday)
                return GetNextMonthlyByPosition(pattern, lastDate);
            }
            else
            {
                // Default: same day next month
                return lastDate.AddMonths(pattern.Interval);
            }
        }
        
        private DateTime GetNextMonthlyByPosition(RecurrencePattern pattern, DateTime lastDate)
        {
            // Default implementation - can be expanded for more complex monthly patterns
            return lastDate.AddMonths(pattern.Interval);
        }
        
        private UserTask CreateTaskFromTemplate(UserTask template, DateTime dueDate)
        {
            return new UserTask
            {
                Title = template.Title,
                Description = template.Description,
                Priority = template.Priority,
                Category = template.Category,
                Tags = template.Tags,
                DueDate = dueDate,
                CreatedAt = DateTime.Now,
                IsCompleted = false,
                UserId = template.UserId
            };
        }
    }
} 
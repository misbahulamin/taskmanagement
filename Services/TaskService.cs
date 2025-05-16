using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserTask>> GetUserTasksAsync(int userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserTask>> GetUserTasksAsync(int userId, bool isCompleted)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted == isCompleted)
                .OrderByDescending(t => isCompleted ? t.CompletedAt : t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserTask>> GetUserTasksByPriorityAsync(int userId, Priority priority)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.Priority == priority)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserTask>> GetUserTasksByCategoryAsync(int userId, string category)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.Category == category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserTask>> SearchUserTasksAsync(int userId, string searchTerm)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && 
                    (t.Title.Contains(searchTerm) || 
                    (t.Description != null && t.Description.Contains(searchTerm)) ||
                    (t.Category != null && t.Category.Contains(searchTerm)) ||
                    (t.Tags != null && t.Tags.Contains(searchTerm))))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserTask?> GetTaskByIdAsync(int taskId, int userId)
        {
            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
        }

        public async Task<UserTask> CreateTaskAsync(UserTask task)
        {
            task.CreatedAt = DateTime.Now;
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<UserTask?> UpdateTaskAsync(UserTask task)
        {
            var existingTask = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == task.Id && t.UserId == task.UserId);

            if (existingTask == null)
            {
                return null;
            }

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.DueDate = task.DueDate;
            existingTask.IsCompleted = task.IsCompleted;
            existingTask.CompletedAt = task.IsCompleted && !existingTask.IsCompleted 
                ? DateTime.Now 
                : (task.IsCompleted ? existingTask.CompletedAt : null);
            existingTask.Priority = task.Priority;
            existingTask.Category = task.Category;
            existingTask.Tags = task.Tags;

            _context.Tasks.Update(existingTask);
            await _context.SaveChangesAsync();
            return existingTask;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkTaskAsCompletedAsync(int taskId, int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null)
            {
                return false;
            }

            task.IsCompleted = true;
            task.CompletedAt = DateTime.Now;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkTaskAsNotCompletedAsync(int taskId, int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null)
            {
                return false;
            }

            task.IsCompleted = false;
            task.CompletedAt = null;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetUserCategoriesAsync(int userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.Category != null)
                .Select(t => t.Category)
                .Distinct()
                .Where(c => c != null)
                .Cast<string>()
                .ToListAsync();
        }
    }
} 
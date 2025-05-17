using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Services
{
    public class CollaborationService : ICollaborationService
    {
        private readonly ApplicationDbContext _context;

        public CollaborationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskCollaborator>> GetTaskCollaboratorsAsync(int taskId)
        {
            return await _context.TaskCollaborators
                .Include(tc => tc.User)
                .Where(tc => tc.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<bool> CanAccessTaskAsync(int taskId, int userId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            
            if (task == null)
                return false;
                
            // User is the owner
            if (task.UserId == userId)
                return true;
                
            // User is a collaborator
            var collaborator = await _context.TaskCollaborators
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.UserId == userId);
                
            return collaborator != null;
        }

        public async Task<bool> CanEditTaskAsync(int taskId, int userId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            
            if (task == null)
                return false;
                
            // User is the owner
            if (task.UserId == userId)
                return true;
                
            // User is a collaborator with edit permissions
            var collaborator = await _context.TaskCollaborators
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.UserId == userId && 
                    (tc.CanEdit || tc.Role == CollaboratorRole.Admin || tc.Role == CollaboratorRole.Editor));
                
            return collaborator != null;
        }

        public async Task<TaskCollaborator> AddCollaboratorAsync(int taskId, int ownerId, int collaboratorId, CollaboratorRole role, bool canEdit)
        {
            // Verify the task exists and belongs to the owner
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == ownerId);
            
            if (task == null)
                throw new InvalidOperationException("Task not found or you don't have permission to share it");
                
            // Verify the collaborator exists
            var collaborator = await _context.Users.FirstOrDefaultAsync(u => u.Id == collaboratorId && u.IsActive);
            
            if (collaborator == null)
                throw new InvalidOperationException("User not found or is inactive");
                
            // Check if already a collaborator
            var existingCollaboration = await _context.TaskCollaborators
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.UserId == collaboratorId);
                
            if (existingCollaboration != null)
                throw new InvalidOperationException("User is already a collaborator for this task");
                
            // Create new collaboration
            var newCollaboration = new TaskCollaborator
            {
                TaskId = taskId,
                UserId = collaboratorId,
                Role = role,
                CanEdit = canEdit,
                AddedAt = DateTime.Now
            };
            
            // Mark the task as shared
            task.IsShared = true;
            
            await _context.TaskCollaborators.AddAsync(newCollaboration);
            await _context.SaveChangesAsync();
            
            return newCollaboration;
        }

        public async Task<bool> RemoveCollaboratorAsync(int taskId, int ownerId, int collaboratorId)
        {
            // Verify the task exists and belongs to the owner
            var task = await _context.Tasks
                .Include(t => t.Collaborators)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == ownerId);
            
            if (task == null)
                return false;
                
            // Find the collaboration
            var collaboration = await _context.TaskCollaborators
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.UserId == collaboratorId);
                
            if (collaboration == null)
                return false;
                
            _context.TaskCollaborators.Remove(collaboration);
            
            // If this was the last collaborator, update IsShared
            if (task.Collaborators?.Count <= 1)
            {
                task.IsShared = false;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCollaboratorAsync(int taskId, int ownerId, int collaboratorId, CollaboratorRole role, bool canEdit)
        {
            // Verify the task exists and belongs to the owner
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == ownerId);
            
            if (task == null)
                return false;
                
            // Find the collaboration
            var collaboration = await _context.TaskCollaborators
                .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.UserId == collaboratorId);
                
            if (collaboration == null)
                return false;
                
            // Update collaboration details
            collaboration.Role = role;
            collaboration.CanEdit = canEdit;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserTask>> GetSharedTasksAsync(int userId)
        {
            // Get all task IDs where the user is a collaborator
            var taskIds = await _context.TaskCollaborators
                .Where(tc => tc.UserId == userId)
                .Select(tc => tc.TaskId)
                .ToListAsync();
                
            if (taskIds.Count == 0)
                return new List<UserTask>();
                
            // Get the full task details
            return await _context.Tasks
                .Include(t => t.User)
                .Where(t => taskIds.Contains(t.Id))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserTask>> GetTasksSharedByUserAsync(int userId)
        {
            return await _context.Tasks
                .Include(t => t.Collaborators)
                .ThenInclude(tc => tc.User)
                .Where(t => t.UserId == userId && t.IsShared)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> FindUserForSharingAsync(string usernameOrEmail)
        {
            return await _context.Users
                .Where(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && u.IsActive)
                .FirstOrDefaultAsync();
        }
    }
} 
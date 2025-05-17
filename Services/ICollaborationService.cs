using TaskManagement.Models;

namespace TaskManagement.Services
{
    public interface ICollaborationService
    {
        // Get collaborators for a specific task
        Task<List<TaskCollaborator>> GetTaskCollaboratorsAsync(int taskId);
        
        // Check if a user has access to a task
        Task<bool> CanAccessTaskAsync(int taskId, int userId);
        
        // Check if a user can edit a task
        Task<bool> CanEditTaskAsync(int taskId, int userId);
        
        // Add a collaborator to a task
        Task<TaskCollaborator> AddCollaboratorAsync(int taskId, int ownerId, int collaboratorId, CollaboratorRole role, bool canEdit);
        
        // Remove a collaborator from a task
        Task<bool> RemoveCollaboratorAsync(int taskId, int ownerId, int collaboratorId);
        
        // Update collaborator's role or permissions
        Task<bool> UpdateCollaboratorAsync(int taskId, int ownerId, int collaboratorId, CollaboratorRole role, bool canEdit);
        
        // Get all tasks shared with a specific user
        Task<List<UserTask>> GetSharedTasksAsync(int userId);
        
        // Get all tasks that a user has shared with others
        Task<List<UserTask>> GetTasksSharedByUserAsync(int userId);
        
        // Find a user by username or email to share a task with
        Task<User?> FindUserForSharingAsync(string usernameOrEmail);
    }
} 
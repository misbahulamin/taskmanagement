using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class UserTask
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;

        public string? Category { get; set; }

        public string? Tags { get; set; } // Comma-separated tags

        // Recurrence properties
        public bool IsRecurring { get; set; } = false;

        // Navigation property to recurrence pattern
        public RecurrencePattern? RecurrencePattern { get; set; }

        // Parent task for recurrence instances
        public int? ParentTaskId { get; set; }

        [ForeignKey("ParentTaskId")]
        public UserTask? ParentTask { get; set; }

        // Child tasks for recurrence instances
        public List<UserTask>? RecurrenceInstances { get; set; }

        // Collaboration properties
        public bool IsShared { get; set; } = false;
        
        // Navigation property for collaborators
        public List<TaskCollaborator>? Collaborators { get; set; }
    }

    public enum Priority
    {
        Low,
        Medium,
        High,
        Urgent
    }
} 
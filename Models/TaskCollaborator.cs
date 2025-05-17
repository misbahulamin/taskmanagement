using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class TaskCollaborator
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public UserTask Task { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public CollaboratorRole Role { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
        
        public bool CanEdit { get; set; }
    }

    public enum CollaboratorRole
    {
        Viewer,     // Can only view task details
        Editor,     // Can edit task details but not delete
        Admin       // Full control (can delete, add other collaborators)
    }
} 
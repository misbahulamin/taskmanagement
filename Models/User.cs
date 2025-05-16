using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string PasswordSalt { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
    }
} 
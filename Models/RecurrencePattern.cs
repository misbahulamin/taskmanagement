using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class RecurrencePattern
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TaskId { get; set; }
        
        [ForeignKey("TaskId")]
        public UserTask? Task { get; set; }
        
        [Required]
        public RecurrenceType Type { get; set; } = RecurrenceType.None;
        
        // Frequency of recurrence (e.g., every 1 day, every 2 weeks)
        [Required]
        public int Interval { get; set; } = 1;
        
        // For weekly recurrence, the days of the week it occurs (bit flag)
        public int? DaysOfWeek { get; set; }
        
        // For monthly recurrence, the day of the month or the ordinal week and day
        public int? DayOfMonth { get; set; }
        
        // For monthly pattern: 1st Monday, 2nd Wednesday, etc.
        public int? WeekOfMonth { get; set; }
        
        // The date when the recurrence starts
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;
        
        // The date when recurrence ends, null for never
        public DateTime? EndDate { get; set; }
        
        // Number of occurrences, null for indefinite
        public int? Occurrences { get; set; }
        
        // Date when the last recurring task was created
        public DateTime? LastGenerated { get; set; }
    }
    
    public enum RecurrenceType
    {
        None = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }
} 
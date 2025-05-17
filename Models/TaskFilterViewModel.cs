using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManagement.Models
{
    public class TaskFilterViewModel
    {
        public string? SearchTerm { get; set; }
        
        public bool? IsCompleted { get; set; }
        
        public Priority? Priority { get; set; }
        
        public string? Category { get; set; }
        
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        
        public List<SelectListItem> Priorities { get; set; }
        
        public List<SelectListItem> CompletionStatuses { get; set; }

        public TaskFilterViewModel()
        {
            // Initialize the Priorities list in the constructor
            Priorities = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Priorities" },
                new SelectListItem { Value = Models.Priority.Low.ToString(), Text = "Low" },
                new SelectListItem { Value = Models.Priority.Medium.ToString(), Text = "Medium" },
                new SelectListItem { Value = Models.Priority.High.ToString(), Text = "High" },
                new SelectListItem { Value = Models.Priority.Urgent.ToString(), Text = "Urgent" }
            };

            // Initialize the CompletionStatuses list in the constructor
            CompletionStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Tasks" },
                new SelectListItem { Value = "false", Text = "Active" },
                new SelectListItem { Value = "true", Text = "Completed" }
            };
        }
    }
} 
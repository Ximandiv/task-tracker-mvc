using System.ComponentModel.DataAnnotations;

namespace Task_Tracker_WebApp.Models.View
{
    public class TaskViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(20)]
        public required string Title { get; set; }

        [StringLength(50)]
        public required string? Description { get; set; }

        [StringLength(50)]
        [Required]
        public required string Status { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime UpdatedDate { get; set; }
    }
}

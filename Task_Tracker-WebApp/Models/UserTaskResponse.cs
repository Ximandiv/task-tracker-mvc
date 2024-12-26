using System.ComponentModel.DataAnnotations;

namespace Task_Tracker_WebApp.Models
{
    public class UserTaskResponse
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Title { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Description { get; set; } = null;

        [StringLength(50)]
        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}

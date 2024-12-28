using System.ComponentModel.DataAnnotations;
using Task_Tracker_WebApp.Database.Entities;

namespace Task_Tracker_WebApp.Models
{
    public class UserTaskModel
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

        public UserTaskModel() { }

        public UserTaskModel(UserTask user)
        {
            Id = user.Id;
            Title = user.Title!;
            Description = user.Description;
            Status = user.Status!;
            CreatedDate = user.CreatedAt;
            UpdatedDate = user.UpdatedAt;
        }
    }
}

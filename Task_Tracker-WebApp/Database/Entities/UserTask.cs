using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Tracker_WebApp.Database.Entities;

public class UserTask
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public required string Title { get; set; }

    [StringLength(50)]
    public required string? Description { get; set; }

    [StringLength(50)]
    public required string Status { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }
}

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
    public required string Description { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime UpdatedDate { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}

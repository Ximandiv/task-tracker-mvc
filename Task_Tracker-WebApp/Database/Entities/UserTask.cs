using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Task_Tracker_WebApp.Models;

namespace Task_Tracker_WebApp.Database.Entities;

public class UserTask
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string? Title { get; set; }

    [StringLength(70)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    public UserTask() { }

    public UserTask
        (UserTaskModel userModel,
        int userId)
    {
        Id = userModel.Id ?? 0;
        Title = userModel.Title;
        Description = userModel.Description;
        Status = userModel.Status;
        UserId = userId;
    }

    public void Update(UserTaskModel userModel)
    {
        Title = userModel.Title;
        Description = userModel.Description;
        Status = userModel.Status;
        UpdatedAt = DateTime.UtcNow;
    }

    public UserTaskModel ToModel()
        => new UserTaskModel(this);
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Task_Tracker_WebApp.Database.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public required string Username { get; set; }

    [Required]
    [StringLength(60)]
    public required string Password { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

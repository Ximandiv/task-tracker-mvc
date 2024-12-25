﻿using System.ComponentModel.DataAnnotations;

namespace Task_Tracker_WebApp.Database.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(25)]
    public required string Username { get; set; }

    [Required]
    [StringLength(200)]
    public required string Password { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
}

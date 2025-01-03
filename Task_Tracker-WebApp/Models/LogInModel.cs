﻿using System.ComponentModel.DataAnnotations;

namespace Task_Tracker_WebApp.Models
{
    public class LogInModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

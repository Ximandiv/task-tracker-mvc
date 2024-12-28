using System.ComponentModel.DataAnnotations;
using Task_Tracker_WebApp.Database.Entities;

namespace Task_Tracker_WebApp.Models
{
    public class SignInModel
    {
        [Required]
        [StringLength(25)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public User MapToUser()
            => new()
            {
                Username = Username!,
                Email = Email!,
                Password = Password!,
            };
    }
}

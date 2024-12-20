using System.ComponentModel.DataAnnotations;

namespace Task_Tracker_WebApp.Models.View
{
    public class SignInViewModel
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
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task_Tracker_WebApp.Database.Entities
{
    public class RememberMeToken
    {
        public int Id { get; set; }

        [Required]
        public required string Token { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime Expiration { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}

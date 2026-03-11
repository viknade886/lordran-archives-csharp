using System.ComponentModel.DataAnnotations;

namespace lordran_archives.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "user";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
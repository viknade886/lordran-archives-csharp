using System.ComponentModel.DataAnnotations;

namespace lordran_archives.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? Image { get; set; }


        public string Status { get; set; } = "pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int SubmittedById { get; set; }
        public User SubmittedBy { get; set; } = null!;
    }
}
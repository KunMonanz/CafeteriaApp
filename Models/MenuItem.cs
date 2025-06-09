using System.ComponentModel.DataAnnotations;

namespace CafeteriaApp.Models
{
    public class MenuItem
    {
        [Key]
        public int MenuItemId { get; set; }

        [Required] // Optional: Add DataAnnotation for validation
        public required string Name { get; set; }

        [Required] // Optional: Add DataAnnotation for validation
        public required string Description { get; set; }

        public decimal Price { get; set; }

        public string? ImagePath { get; set; }
    }
}
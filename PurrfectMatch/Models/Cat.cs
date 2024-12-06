using System.ComponentModel.DataAnnotations;

namespace PurrfectMatch.Models
{
    public class Cat
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, 20, ErrorMessage = "Wiek musi być pomiędzy 0 a 20 lat.")]
        public int Age { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? Diseases { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }
}

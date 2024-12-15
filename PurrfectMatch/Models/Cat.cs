using System.ComponentModel.DataAnnotations;

namespace PurrfectMatch.Models
{
    public class Cat
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Imię")]
        public string Name { get; set; }

        [Required]
        [Range(0, 20, ErrorMessage = "Wiek musi być pomiędzy 0 a 20 lat.")]
        [Display(Name = "Wiek")]
        public int Age { get; set; }

        [StringLength(500)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Zdjęcie")]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Choroby")]
        public string? Diseases { get; set; }

        [Required]
        [Display(Name = "Dostępność")]
        public bool IsAvailable { get; set; }

        [Required]
        [Display(Name = "Płeć")]
        public string Gender { get; set; }
    }
}

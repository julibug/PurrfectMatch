using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PurrfectMatch.Models
{
    public class RegisterViewModel 
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Are you 18 or older?")]
        public bool IsAdult { get; set; }
    }
}

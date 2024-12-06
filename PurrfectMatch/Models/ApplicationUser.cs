using Microsoft.AspNetCore.Identity;

namespace PurrfectMatch.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? isAdult{ get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Models;
using PurrfectMatch.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace PurrfectMatch.Controllers
{
    [Authorize]
    public class AdoptionController : Controller
    {
        private readonly CatDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdoptionController(CatDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult RequestAdoption(int catId)
        {
            // Tworzymy pusty model, aby przekazać ID kota do widoku
            var model = new AdoptionRequest
            {
                CatId = catId // Ustawiamy ID kota, który ma być adoptowany
            };

            return View(model); // Przekazujemy model do widoku
        }

        [HttpPost]
        public async Task<IActionResult> RequestAdoption(AdoptionRequest model)
        {
            // Pobierz zalogowanego użytkownika
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("User is not authenticated");
                return Unauthorized();
            }

            // Przypisz UserId i usuń ewentualne błędy walidacji dla tego pola
            model.UserId = user.Id;
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                // Dodaj model do bazy danych
                _context.AdoptionRequests.Add(model);
                await _context.SaveChangesAsync();
                Console.WriteLine("Request saved successfully!");
                return RedirectToAction("Confirmation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
                return View(model);
            }
        }

        public IActionResult Confirmation()
        {
            return View();
        }

    }
}

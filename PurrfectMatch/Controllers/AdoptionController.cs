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
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingRequest = _context.AdoptionRequests
                .FirstOrDefault(r => r.UserId == user.Id && r.CatId == catId);

            if (existingRequest != null)
            {
                ViewBag.Message = "Twój wniosek został już złożony.";
                return View("AdoptionRequestAlreadySubmitted");
            }

            var model = new AdoptionRequest
            {
                CatId = catId 
            };

            return View(model); 
        }

        [HttpPost]
        public async Task<IActionResult> RequestAdoption(AdoptionRequest model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("User is not authenticated");
                return Unauthorized();
            }

            var existingRequest = await _context.AdoptionRequests
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.CatId == model.CatId);

            if (existingRequest != null)
            {
                ModelState.AddModelError(string.Empty, "Twój wniosek został już złożony dla tego kota.");
                return View(model); 
            }

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

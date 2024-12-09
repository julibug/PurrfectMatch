using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Data;
using PurrfectMatch.Models;

namespace PurrfectMatch.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly CatDbContext _catDbContext;
        private readonly ApplicationDbContext _applicationDbContext;

        public AdminController(CatDbContext catDbContext, ApplicationDbContext applicationDbContext)
        {
            _catDbContext = catDbContext;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IActionResult> Index()
        {
            // Pobierz dane o wnioskach adopcyjnych i kotach
            var adoptionRequests = await _catDbContext.AdoptionRequests.ToListAsync();
            var cats = await _catDbContext.Cats.ToListAsync();

            // Utwórz widok model z odpowiednimi danymi
            var model = adoptionRequests.Select(ar => new AdoptionRequestAdminViewModel
            {
                RequestId = ar.Id,
                UserName = _applicationDbContext.Users.FirstOrDefault(u => u.Id == ar.UserId)?.UserName ?? "Brak użytkownika",
                CatName = cats.FirstOrDefault(c => c.Id == ar.CatId)?.Name ?? "Nieznany kot",
                CatId = ar.CatId,
                HasOtherAnimals = ar.HasOtherAnimals,
                HasChildren = ar.HasChildren,
                Housing = ar.Housing
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId, int catId)
        {
            Console.WriteLine($"RequestId: {requestId}, CatId: {catId}");

            // Pobierz kota z bazy danych
            var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == catId);

            // Sprawdź, czy kot istnieje
            if (cat == null)
            {
                TempData["ErrorMessage"] = "Kot nie istnieje.";
                return RedirectToAction("Index");
            }

            // Sprawdź, czy kot jest dostępny
            if (!cat.IsAvailable)
            {
                TempData["ErrorMessage"] = "Kot jest już niedostępny.";
                return RedirectToAction("Index");
            }

            // Oznacz kota jako niedostępnego
            cat.IsAvailable = false;

            // Usuń wniosek adopcyjny z bazy danych
            var request = await _catDbContext.AdoptionRequests.FirstOrDefaultAsync(ar => ar.Id == requestId);
            if (request != null)
            {
                _catDbContext.AdoptionRequests.Remove(request);
            }

            // Zapisz zmiany w bazie danych
            await _catDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wniosek został zaakceptowany. Kot został oznaczony jako niedostępny.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            // Usuń wniosek adopcyjny
            var request = await _catDbContext.AdoptionRequests.FirstOrDefaultAsync(ar => ar.Id == requestId);
            if (request != null)
            {
                _catDbContext.AdoptionRequests.Remove(request);
                await _catDbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}

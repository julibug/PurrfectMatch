using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        // Dodaj UserManager do konstruktora kontrolera
        public AdminController(CatDbContext catDbContext, ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            _catDbContext = catDbContext;
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Pobierz wnioski adopcyjne, tylko te o statusie "Oczekujący"
            var requests = await _catDbContext.AdoptionRequests
                .Where(r => r.Status == "Oczekujący")  // Tylko wnioski oczekujące
                .Select(r => new AdoptionRequestAdminViewModel
                {
                    RequestId = r.Id,
                    UserId = r.UserId,  // Dodaj UserId w modelu
                    UserName = "", // Inicjalizujemy UserName, a potem zaktualizujemy go później
                    CatName = _catDbContext.Cats.FirstOrDefault(c => c.Id == r.CatId).Name,  // Pobierz nazwę kota
                    HasOtherAnimals = r.HasOtherAnimals,
                    HasChildren = r.HasChildren,
                    Housing = r.Housing,
                    CatId = r.CatId
                })
                .ToListAsync();

            // Pobierz dane użytkowników na podstawie UserId
            var userIds = requests.Select(r => r.UserId).Distinct().ToList();
            var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            // Przypisz UserName do wniosków adopcyjnych
            foreach (var request in requests)
            {
                var user = users.FirstOrDefault(u => u.Id == request.UserId);
                if (user != null)
                {
                    request.UserName = user.UserName;  // Przypisujemy UserName użytkownika
                }
            }

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId, int catId)
        {
            // Pobierz wniosek adopcyjny
            var request = await _catDbContext.AdoptionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            // Pobierz kota z bazy danych
            var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == catId);

            if (cat == null || request == null)
            {
                TempData["ErrorMessage"] = "Wniosek lub kot nie istnieje.";
                return RedirectToAction("Index");
            }

            // Sprawdź, czy kot jest dostępny
            if (!cat.IsAvailable)
            {
                TempData["ErrorMessage"] = "Kot jest już niedostępny.";
                return RedirectToAction("Index");
            }

            // Zmiana statusu wniosku na zaakceptowany
            request.Status = "Zaakceptowany";

            // Oznacz kota jako niedostępnego
            cat.IsAvailable = false;

            // Zaktualizuj dane w bazie
            _catDbContext.AdoptionRequests.Update(request);
            _catDbContext.Cats.Update(cat);

            await _catDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wniosek został zaakceptowany, skontaktujemy się z użytkownikiem.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId, string rejectionReason)
        {
            var request = await _catDbContext.AdoptionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Wniosek nie istnieje.";
                return RedirectToAction("Index");
            }

            // Przypisz powód odrzucenia do wniosku
            request.Status = "Odrzucony";
            request.RejectionReason = rejectionReason;

            // Zaktualizuj wniosek w bazie danych
            _catDbContext.AdoptionRequests.Update(request);
            await _catDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wniosek został odrzucony.";
            return RedirectToAction("Index");
        }
    }
}

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

        public async Task<IActionResult> Index(string status = "All")
        {
            // Zacznij od zapytania do AdoptionRequests
            IQueryable<AdoptionRequest> requestsQuery = _catDbContext.AdoptionRequests;

            // Filtrowanie wniosków na podstawie statusu
            if (status == "Rozpatrzone")
            {
                requestsQuery = requestsQuery.Where(r => r.Status == "Zaakceptowany" || r.Status == "Odrzucony");
            }
            else if (status == "Nierozpatrzone")
            {
                requestsQuery = requestsQuery.Where(r => r.Status == "Oczekujący");
            }
            // Jeśli status = "All" (domyślnie), wtedy nie stosujemy żadnego filtra

            // Pobierz wnioski z bazy danych na podstawie filtrowania
            var requests = await requestsQuery
                .Select(r => new AdoptionRequestAdminViewModel
                {
                    RequestId = r.Id,
                    UserId = r.UserId,
                    UserName = "", // Przypiszemy później UserName
                    CatName = _catDbContext.Cats.FirstOrDefault(c => c.Id == r.CatId).Name,
                    HasOtherAnimals = r.HasOtherAnimals,
                    HasChildren = r.HasChildren,
                    Housing = r.Housing,
                    CatId = r.CatId,
                    Status = r.Status,  // Pobierz status
                    RejectionReason = r.RejectionReason  // Pobierz powód odrzucenia
                })
                .ToListAsync();

            // Pobieramy użytkowników na podstawie UserId
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
            var approvedRequest = await _catDbContext.AdoptionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            // Pobierz kota z bazy danych
            var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == catId);

            if (cat == null || approvedRequest == null)
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

            // Zmiana statusu zaakceptowanego wniosku
            approvedRequest.Status = "Zaakceptowany";

            // Oznacz kota jako niedostępnego
            cat.IsAvailable = false;

            // Pobierz inne wnioski dotyczące tego samego kota
            var otherRequests = await _catDbContext.AdoptionRequests
                .Where(r => r.CatId == catId && r.Id != requestId && r.Status == "Oczekujący")
                .ToListAsync();

            // Odrzuć wszystkie inne wnioski
            foreach (var request in otherRequests)
            {
                request.Status = "Odrzucony";
                request.RejectionReason = "Przepraszamy, ale kotek został już zarezerwowany do adopcji.";
                _catDbContext.AdoptionRequests.Update(request);
            }

            // Zaktualizuj zaakceptowany wniosek i kota
            _catDbContext.AdoptionRequests.Update(approvedRequest);
            _catDbContext.Cats.Update(cat);

            // Zapisz zmiany w bazie danych
            await _catDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wniosek został zaakceptowany. Pozostałe wnioski zostały automatycznie odrzucone.";
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

        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int requestId)
        {
            // Pobierz wniosek adopcyjny z bazy danych
            var request = await _catDbContext.AdoptionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Wniosek nie istnieje.";
                return RedirectToAction("Index");
            }

            try
            {
                // Usuń wniosek z bazy danych
                _catDbContext.AdoptionRequests.Remove(request);
                await _catDbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Wniosek został pomyślnie usunięty.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas usuwania wniosku: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}

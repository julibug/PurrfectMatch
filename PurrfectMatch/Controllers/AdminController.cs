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

        public AdminController(CatDbContext catDbContext, ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            _catDbContext = catDbContext;
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string status = "All")
        {
            IQueryable<AdoptionRequest> requestsQuery = _catDbContext.AdoptionRequests;

            if (status == "Rozpatrzone")
            {
                requestsQuery = requestsQuery.Where(r => r.Status == "Zaakceptowany" || r.Status == "Odrzucony");
            }
            else if (status == "Nierozpatrzone")
            {
                requestsQuery = requestsQuery.Where(r => r.Status == "Oczekujący");
            }

            var requests = await requestsQuery
                .Select(r => new AdoptionRequestAdminViewModel
                {
                    RequestId = r.Id,
                    UserId = r.UserId,
                    UserName = "",
                    CatName = _catDbContext.Cats.FirstOrDefault(c => c.Id == r.CatId).Name,
                    HasOtherAnimals = r.HasOtherAnimals,
                    HasChildren = r.HasChildren,
                    Housing = r.Housing,
                    CatId = r.CatId,
                    Status = r.Status,  
                    RejectionReason = r.RejectionReason,
                    AdoptionReason = r.AdoptionReason
                })
                .ToListAsync();

            var userIds = requests.Select(r => r.UserId).Distinct().ToList();
            var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            foreach (var request in requests)
            {
                var user = users.FirstOrDefault(u => u.Id == request.UserId);
                if (user != null)
                {
                    request.UserName = user.UserName;  
                }
            }

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId, int catId)
        {
            var approvedRequest = await _catDbContext.AdoptionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == catId);

            if (cat == null || approvedRequest == null)
            {
                TempData["ErrorMessage"] = "Wniosek lub kot nie istnieje.";
                return RedirectToAction("Index");
            }

            if (!cat.IsAvailable)
            {
                TempData["ErrorMessage"] = "Kot jest już niedostępny.";
                return RedirectToAction("Index");
            }

            approvedRequest.Status = "Zaakceptowany";

            cat.IsAvailable = false;

            var notification = new Notification
            {
                UserId = approvedRequest.UserId,
                Message = $"Twój wniosek adopcyjny dla kota {cat.Name} został rozpatrzony.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _catDbContext.Notifications.Add(notification);

            var otherRequests = await _catDbContext.AdoptionRequests
                .Where(r => r.CatId == catId && r.Id != requestId && r.Status == "Oczekujący")
                .ToListAsync();
            
            foreach (var request in otherRequests)
            {
                request.Status = "Odrzucony";
                request.RejectionReason = "Przepraszamy, ale kotek został już zarezerwowany do adopcji.";
                _catDbContext.AdoptionRequests.Update(request);

                var rejectionNotification = new Notification
                {
                    UserId = request.UserId,
                    Message = $"Twój wniosek adopcyjny dla kota {cat.Name} został rozpatrzony.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _catDbContext.Notifications.Add(rejectionNotification);
            }

            _catDbContext.AdoptionRequests.Update(approvedRequest);
            _catDbContext.Cats.Update(cat);

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

            request.Status = "Odrzucony";
            request.RejectionReason = rejectionReason;

            // Powiadomienie dla odrzuconego wniosku
            var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == request.CatId);
            var notification = new Notification
            {
                UserId = request.UserId,
                Message = $"Twój wniosek adopcyjny dla kota {cat.Name} został rozpatrzony.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _catDbContext.Notifications.Add(notification);

            _catDbContext.AdoptionRequests.Update(request);
            await _catDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wniosek został odrzucony.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int requestId)
        {
            var request = await _catDbContext.AdoptionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Wniosek nie istnieje.";
                return RedirectToAction("Index");
            }

            try
            {
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

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var nonAdminUsers = new List<ApplicationUser>();
            foreach (var user in users)
            {
                if (!await _userManager.IsInRoleAsync(user, "Administrator"))
                {
                    nonAdminUsers.Add(user);
                }
            }

            return View(nonAdminUsers); 
        }

        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.UserName = model.UserName;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var adoptionRequests = await _catDbContext.AdoptionRequests
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            _catDbContext.AdoptionRequests.RemoveRange(adoptionRequests);
            await _catDbContext.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ManageUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }
    }
}

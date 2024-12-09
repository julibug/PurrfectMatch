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
                HasOtherAnimals = ar.HasOtherAnimals,
                HasChildren = ar.HasChildren,
                Housing = ar.Housing
            }).ToList();

            return View(model);
        }
    }
}

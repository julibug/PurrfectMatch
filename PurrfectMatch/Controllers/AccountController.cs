using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Data;
using PurrfectMatch.Models;
using System.Threading.Tasks;

namespace PurrfectMatch.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CatDbContext _catDbContext;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, CatDbContext catDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _catDbContext = catDbContext;
        }

        // Akcja rejestracji
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Zalogowanie użytkownika po rejestracji
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home"); // Przekierowanie na stronę główną po rejestracji
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        // Akcja logowania POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        // Jeżeli logowanie jest udane, wypisujemy komunikat w konsoli
                        Console.WriteLine("Login Successful");

                        // Przekierowanie na stronę z kotami
                        return RedirectToAction("Index", "Cats");
                    }
                    else
                    {
                        // Jeśli logowanie się nie powiodło, wypisujemy błąd
                        Console.WriteLine("Login failed: Invalid credentials.");
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    // Jeśli nie znaleziono użytkownika
                    Console.WriteLine("Login failed: User not found.");
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        // Akcja wylogowania
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Cats");
        }
        //[Authorize]
        //public IActionResult Details()
        //{
        //    // Pobranie informacji o aktualnym użytkowniku
        //    var userName = User.Identity.Name; // Nazwa użytkownika
        //    var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(); // Wszystkie dane z claimów

        //    // Przekazanie danych do widoku
        //    return View(userClaims);
        //}

        //[Authorize]
        //public async Task<IActionResult> Profile()
        //{
        //    // Pobieramy zalogowanego użytkownika
        //    var user = await _userManager.GetUserAsync(User);

        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // Pobieramy wszystkie wnioski adopcyjne danego użytkownika
        //    var adoptionRequests = await _catDbContext.AdoptionRequests
        //        .Where(r => r.UserId == user.Id)
        //        .ToListAsync();

        //    // Tworzymy listę do widoku
        //    var profileViewModel = new List<UserAdoptionRequestViewModel>();

        //    foreach (var request in adoptionRequests)
        //    {
        //        // Pobieramy dane o kocie na podstawie CatId
        //        var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == request.CatId);

        //        if (cat != null) // Jeśli kot istnieje
        //        {
        //            profileViewModel.Add(new UserAdoptionRequestViewModel
        //            {
        //                CatName = cat.Name,
        //                Status = request.Status // Status wniosku adopcyjnego
        //            });
        //        }
        //    }

        //    // Przekazujemy model do widoku
        //    return View(profileViewModel);
        //}

        [Authorize]
        public async Task<IActionResult> Details()
        {
            // Pobranie aktualnego użytkownika
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Pobieramy wszystkie wnioski adopcyjne danego użytkownika
            var adoptionRequests = await _catDbContext.AdoptionRequests
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            // Tworzymy listę do widoku
            var userAdoptionRequestViewModels = new List<UserAdoptionRequestViewModel>();

            foreach (var request in adoptionRequests)
            {
                // Pobieramy dane o kocie na podstawie CatId
                var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == request.CatId);

                if (cat != null) // Jeśli kot istnieje
                {
                    userAdoptionRequestViewModels.Add(new UserAdoptionRequestViewModel
                    {
                        CatName = cat.Name,
                        Status = request.Status // Status wniosku adopcyjnego
                    });
                }
            }

            // Przekazujemy dane użytkownika oraz wnioski adopcyjne do widoku
            ViewBag.UserName = user.UserName;
            ViewBag.UserEmail = user.Email;

            return View(userAdoptionRequestViewModels);
        }
    }
}

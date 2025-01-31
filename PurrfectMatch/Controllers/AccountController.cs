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
        // Zarządza użytkownikami
        private readonly UserManager<ApplicationUser> _userManager;

        // Zarządza procesem logowania użytkowników
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Kontekst bazy danych dla aplikacji, używany do obsługi adopcji kotów
        private readonly CatDbContext _catDbContext;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, CatDbContext catDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _catDbContext = catDbContext;
        }

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
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home"); 
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
                        Console.WriteLine("Login Successful");

                        return RedirectToAction("Index", "Cats");
                    }
                    else
                    {
                        Console.WriteLine("Login failed: Invalid credentials.");
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    Console.WriteLine("Login failed: User not found.");
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Cats");
        }

        [Authorize]
        public async Task<IActionResult> Details()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var adoptionRequests = await _catDbContext.AdoptionRequests
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            var userAdoptionRequestViewModels = new List<UserAdoptionRequestViewModel>();

            foreach (var request in adoptionRequests)
            {
                var cat = await _catDbContext.Cats.FirstOrDefaultAsync(c => c.Id == request.CatId);

                if (cat != null) 
                {
                    userAdoptionRequestViewModels.Add(new UserAdoptionRequestViewModel
                    {
                        CatName = cat.Name,
                        Status = request.Status, 
                        RejectionReason = request.Status == "Odrzucony" ? request.RejectionReason : null
                    });
                }
            }

            ViewBag.UserName = user.UserName;
            ViewBag.UserEmail = user.Email;

            return View(userAdoptionRequestViewModels);
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PurrfectMatch.Models;
using System.Threading.Tasks;

namespace PurrfectMatch.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Akcja rejestracji
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Po rejestracji, przekierowanie do strony logowania
                    return RedirectToAction("Login", "Account");
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
            return RedirectToAction("Index", "Home");
        }
    }
}

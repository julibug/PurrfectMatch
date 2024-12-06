using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using PurrfectMatch.Data;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Hosting;
using PurrfectMatch.Models;

namespace PurrfectMatch.Controllers
{
    public class CatsController : Controller
    {
        private readonly CatDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CatsController(CatDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // Akcja dodawania kota
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Cat cat, IFormFile imageFile) // imageFile to plik przesłany przez użytkownika
        {
            if (ModelState.IsValid)
            {
                // Obsługa przesyłania zdjęcia
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Generowanie ścieżki, w której zapisujemy plik
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", imageFile.FileName);

                    // Zapisz plik na serwerze
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Przypisz ścieżkę do zdjęcia w modelu kota
                    cat.ImageUrl = "/images/" + imageFile.FileName;
                }

                // Zapisz kota do bazy danych
                _context.Cats.Add(cat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Po dodaniu przekieruj na listę kotów
            }

            return View(cat);
        }

        // Akcja, która wyświetla listę kotów
        public IActionResult Index()
        {
            var cats = _context.Cats.ToList(); // Pobieramy wszystkich kotów z bazy danych
            return View(cats); // Zwracamy widok z listą kotów
        }

        public IActionResult Details(int id)
        {
            var cat = _context.Cats.FirstOrDefault(c => c.Id == id);
            if (cat == null)
            {
                return NotFound();
            }
            return View(cat);
        }
    }
}

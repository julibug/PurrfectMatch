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
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create(Cat cat, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(stream);
                        stream.Position = 0;

                        // Przycinanie i skalowanie obrazu
                        using (var image = Image.Load(stream))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Size = new Size(300, 300), // np. kwadrat 300x300
                                Mode = ResizeMode.Crop
                            }));
                            image.Save(filePath); // Zapis przyciętego obrazu
                        }
                    }

                    cat.ImageUrl = "/images/" + uniqueFileName;
                }

                _context.Cats.Add(cat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(cat);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cat = _context.Cats.FirstOrDefault(c => c.Id == id);
            if (cat == null)
            {
                return NotFound();
            }
            return View(cat);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Cat cat, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingCat = _context.Cats.FirstOrDefault(c => c.Id == id);
                if (existingCat == null)
                {
                    return NotFound();
                }

                existingCat.Name = cat.Name;
                existingCat.Description = cat.Description;
                existingCat.Age = cat.Age;
                existingCat.IsAvailable = cat.IsAvailable;
                existingCat.Diseases = cat.Diseases;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(stream);
                        stream.Position = 0;

                        using (var image = Image.Load(stream))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Size = new Size(300, 300),
                                Mode = ResizeMode.Crop
                            }));
                            image.Save(filePath);
                        }
                    }

                    existingCat.ImageUrl = "/images/" + uniqueFileName;
                }

                _context.Update(existingCat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(cat);
        }

        // Akcja, która wyświetla formularz potwierdzenia usunięcia kota
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var cat = _context.Cats.FirstOrDefault(c => c.Id == id);
            if (cat == null)
            {
                return NotFound();
            }

            return View(cat);  // Zwracamy widok, aby użytkownik mógł potwierdzić usunięcie
        }

        // Akcja, która faktycznie usuwa kota z bazy danych
        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cat = await _context.Cats.FindAsync(id);
            if (cat == null)
            {
                return NotFound();
            }

            _context.Cats.Remove(cat);  // Usuwamy kota
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));  // Po usunięciu przekierowujemy na stronę z listą kotów
        }

        // Akcja wyświetlająca listę kotów
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
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
                existingCat.Gender = cat.Gender; 

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

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var cat = _context.Cats.FirstOrDefault(c => c.Id == id);
            if (cat == null)
            {
                return NotFound();
            }

            return View(cat); 
        }

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

            _context.Cats.Remove(cat);  
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); 
        }

        public IActionResult Index()
        {
            var cats = _context.Cats.ToList(); 
            return View(cats);
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
using Microsoft.AspNetCore.Mvc;
using PurrfectMatch.Data;
using System.Text.Encodings.Web;

namespace PurrfectMatch.Controllers
{
    public class CatsController : Controller
    {
        private readonly CatDbContext _context;

        public CatsController(CatDbContext context)
        {
            _context = context;
        }

        // Akcja, która wyświetla listę kotów
        public IActionResult Index()
        {
            var cats = _context.Cats.ToList(); // Pobieramy wszystkich kotów z bazy danych
            return View(cats); // Zwracamy widok z listą kotów
        }
    }
}

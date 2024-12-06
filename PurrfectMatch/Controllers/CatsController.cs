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

        public IActionResult Index()
        {
            var cats = _context.Cats.ToList();
            return View(cats); // Przekaż dane do widoku
        }
    }
}

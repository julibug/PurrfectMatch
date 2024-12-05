using Microsoft.AspNetCore.Mvc;
using PurrfectMatch.Models;
using PurrfectMatch.Data;
using System.Collections.Generic;
using System.Linq;

namespace PurrfectMatch.Controllers
{
    [Route("Cats")]
    public class CatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cats/GetAvailableCats
        [HttpGet("GetAvailableCats")]
        public IActionResult GetAvailableCats()
        {
            var cats = _context.Cats
                .Where(c => !c.IsReserved) // Filtruj koty nie zarezerwowane
                .ToList();

            return Json(cats);
        }

        // POST: /Cats/AddCat
        [HttpPost("AddCat")]
        public IActionResult AddCat([FromBody] Cat newCat)
        {
            if (newCat == null)
            {
                return BadRequest("Invalid cat data.");
            }

            _context.Cats.Add(newCat);
            _context.SaveChanges();

            return Ok(new { message = "Cat added successfully!" });
        }
    }
}

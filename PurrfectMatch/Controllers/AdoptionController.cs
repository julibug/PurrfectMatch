using Microsoft.AspNetCore.Mvc;

namespace PurrfectMatch.Controllers
{
    public class AdoptionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

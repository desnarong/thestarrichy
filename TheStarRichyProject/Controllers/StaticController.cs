using Microsoft.AspNetCore.Mvc;

namespace TheStarRichyProject.Controllers
{
    public class StaticController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

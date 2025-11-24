using Microsoft.AspNetCore.Mvc;

namespace TheStarRichyProject.Controllers
{
    public class buyordersController : Controller
    {
        public IActionResult buyorderbyewallet()
        {
            return View();
        }
        public IActionResult saleorderfromhold()
        {
            return View();
        }
        public IActionResult ewallettransfer()
        {
            return View();
        }
    }
}

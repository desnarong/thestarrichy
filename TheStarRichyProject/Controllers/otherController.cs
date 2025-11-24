using Microsoft.AspNetCore.Mvc;

namespace TheStarRichyProject.Controllers
{
    public class otherController : Controller
    {
        public IActionResult changepassword()
        {
            return View();
        }
        public IActionResult taxdownload()
        {
            return View();
        }
        public IActionResult documents()
        {
            return View();
        }
    }
}

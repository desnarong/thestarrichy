using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TheStarRichyProject.Controllers
{
    public class ExternalRegistrationController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Return the standalone external registration view (no layout)
            return View("~/Views/ExternalRegistration/Index.cshtml");
        }
    }
}

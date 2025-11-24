using Microsoft.AspNetCore.Mvc;
using TheStarRichyProject.Helper;

namespace TheStarRichyProject.Controllers
{
    public class BaseController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected IConfiguration Config { get; private set; }
        public BaseController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration config)
        {
            Config = config;
            _httpContextAccessor = httpContextAccessor;
        }
        public ActionResult CheckCookie()
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies[CookieHelper.UserKey];
            if (string.IsNullOrEmpty(cookie))
            {
                return Redirect("/Auth/Login");
            }
            return null;
        }
    }
}

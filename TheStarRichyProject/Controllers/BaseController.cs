using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TheStarRichyProject.Helper;

namespace TheStarRichyProject.Controllers
{
    [SessionCheck]
    public class BaseController : Controller
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILogger _logger;
        protected readonly IConfiguration _config;

        public BaseController(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = loggerFactory.CreateLogger(GetType());
            _config = config;
        }

        protected string GetUserSession()
        {
            return Request.Cookies["UserSession"];
        }

        protected bool IsSessionValid()
        {
            return !string.IsNullOrEmpty(GetUserSession());
        }

        protected bool CheckCookie(out string userCode)
        {
            userCode = string.Empty;
            
            // Check if UserSession cookie exists (contains JWT token)
            var cookieToken = Request.Cookies["UserSession"];
            if (string.IsNullOrEmpty(cookieToken))
            {
                return false;
            }
            
            // Get MemberCode from separate cookie (stored during login)
            userCode = Request.Cookies[CookieHelper.MemberCodeKey] ?? string.Empty;
            
            // Valid if we have both session token and member code
            return !string.IsNullOrEmpty(userCode);
        }

        protected string GetClientIPAddress()
        {
            string ip = string.Empty;
            
            if (_httpContextAccessor.HttpContext != null)
            {
                ip = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(ip))
                {
                    ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                }
            }

            return ip ?? "127.0.0.1";
        }

        protected IConfiguration Config => _config;
    }

    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Skip for public paths
            var path = context.HttpContext.Request.Path.Value ?? "";
            var controller = context.Controller.GetType().Name;
            
            // Skip AuthController and ExternalRegistrationController entirely (login, register, etc.)
            if (controller.Equals("AuthController", StringComparison.OrdinalIgnoreCase) ||
                controller.Equals("ExternalRegistrationController", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(context);
                return;
            }
            
            var isPublicPath =
                path.StartsWith("/Auth/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/ExternalRegistration/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/Culture/", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/home/GetSlideImages", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/home/GetPopupSlideImages", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/Home/Error", StringComparison.OrdinalIgnoreCase);

            if (isPublicPath)
            {
                base.OnActionExecuting(context);
                return;
            }

            var userSession = context.HttpContext.Request.Cookies["UserSession"];

            if (string.IsNullOrEmpty(userSession))
            {
                var acceptHeader = context.HttpContext.Request.Headers["Accept"].ToString();
                var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                    || (acceptHeader.Contains("application/json") && !acceptHeader.Contains("text/html"));

                if (isAjax)
                {
                    context.Result = new JsonResult(new { error = "session_expired", message = "กรุณาเข้าสู่ระบบใหม่" })
                    {
                        StatusCode = 401
                    };
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}

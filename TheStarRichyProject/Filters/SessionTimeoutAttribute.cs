using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TheStarRichyProject.Filters
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
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

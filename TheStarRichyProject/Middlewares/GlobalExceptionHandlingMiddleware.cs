using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace TheStarRichyProject.Middlewares
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                
                // Check if it's an AJAX request
                var acceptHeader = context.Request.Headers["Accept"].ToString();
                var isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                    || (acceptHeader.Contains("application/json") && !acceptHeader.Contains("text/html"));

                if (isAjax)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync("{\"error\":\"internal_error\",\"message\":\"เกิดข้อผิดพลาดในระบบ\"}");
                }
                else
                {
                    // Redirect to error page with session expired flag
                    context.Response.Redirect("/Home/Error?sessionExpired=true");
                }
            }
        }
    }
}

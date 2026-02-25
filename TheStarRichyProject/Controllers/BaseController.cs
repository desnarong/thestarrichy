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

        /// <summary>
        /// ดึง IP Address ของ client โดยตรวจสอบจาก headers ต่างๆ ด้วย loop
        /// </summary>
        protected string GetClientIPAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "0";

            // รายการ headers ที่ใช้ตรวจสอบ IP address (เรียงลำดับความสำคัญ)
            var ipHeaders = new[]
            {
                "X-Forwarded-For",
                "X-Real-IP",
                "CF-Connecting-IP", // Cloudflare
                "X-Client-IP",
                "X-Cluster-Client-IP",
                "Forwarded-For",
                "Forwarded"
            };

            // ใช้ loop ตรวจสอบแต่ละ header
            foreach (var headerName in ipHeaders)
            {
                var headerValue = httpContext.Request.Headers[headerName].FirstOrDefault();
                if (!string.IsNullOrEmpty(headerValue))
                {
                    // header อาจมีหลาย IP คั่นด้วย comma (เช่น X-Forwarded-For: client, proxy1, proxy2)
                    var ips = headerValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var ip in ips)
                    {
                        var cleanIp = ip.Trim();
                        
                        // ตรวจสอบว่าเป็น IP address ที่ valid และไม่ใช่ localhost
                        if (IsValidIpAddress(cleanIp) && !IsLocalhost(cleanIp))
                        {
                            return cleanIp;
                        }
                    }
                }
            }

            // ถ้าไม่มี header ที่ valid ให้ใช้ RemoteIpAddress
            var remoteIp = httpContext.Connection.RemoteIpAddress;
            if (remoteIp != null)
            {
                var ipString = remoteIp.ToString();
                
                // แปลง IPv6 loopback เป็น IPv4 loopback
                if (IsLocalhost(ipString))
                    return "127.0.0.1";
                    
                // แปลง IPv6 mapped IPv4 address
                if (remoteIp.IsIPv4MappedToIPv6)
                    return remoteIp.MapToIPv4().ToString();
                    
                return ipString;
            }

            return "0";
        }

        /// <summary>
        /// ตรวจสอบว่าเป็น IP address ที่ valid หรือไม่
        /// </summary>
        private bool IsValidIpAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return false;

            // ตรวจสอบรูปแบบ IP address (ทั้ง IPv4 และ IPv6)
            return System.Net.IPAddress.TryParse(ip, out _);
        }

        /// <summary>
        /// ตรวจสอบว่าเป็น localhost address หรือไม่
        /// </summary>
        private bool IsLocalhost(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return false;

            // รายการ localhost addresses
            var localhostAddresses = new[]
            {
                "::1",
                "127.0.0.1",
                "localhost",
                "0:0:0:0:0:0:0:1"
            };

            // ตรวจสอบว่า IP ขึ้นต้นด้วย 2001: (IPv6 local range) หรือไม่
            if (ip.StartsWith("2001:"))
                return true;

            // ตรวจสอบว่าเป็น localhost address หรือไม่
            return localhostAddresses.Contains(ip) || 
                   ip.StartsWith("127.") || // IPv4 loopback range
                   ip.StartsWith("fe80:"); // IPv6 link-local
        }
    }
}

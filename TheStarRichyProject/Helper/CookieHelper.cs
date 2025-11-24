namespace TheStarRichyProject.Helper
{
    public static class CookieHelper
    {
        public static string LanguageKey = "language";
        public static string MemberCodeKey = "MemberCode";
        public static string UserKey = "UserSession";
        public static string UserInfoKey = "UserInfo";
        public static string MessageInfoKey = "RunMessage";
        public static string SystemInfoKey = "Systemname";
        public static string PermissionsKey = "permissions";
        public static string PagesKey = "pages";
        public static string MemberPositionPictureKey = "MemberPositionPicture";
        public static string MemberInfoKey = "memberinfo";
        public static void SetCookie(this IHttpContextAccessor httpContextAccessor, string key, string value, TimeSpan expires)
        {
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                IsEssential = true,
                Expires = DateTime.Now.Add(expires)
            };

            httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }
        public static void SetCookie(this IHttpContextAccessor httpContextAccessor, string key, string value, CookieOptions options)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, options);
        }
        public static string GetCookie(this IHttpContextAccessor httpContextAccessor, string key)
        {
            return httpContextAccessor.HttpContext.Request.Cookies[key];
        }
        public static string ClearCookie(this IHttpContextAccessor httpContextAccessor, string key)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
            return "ok";
        }
        public static bool CheckCookieExpiration(this IHttpContextAccessor httpContextAccessor, string key)
        {
            var cookie = httpContextAccessor.HttpContext.Request.Cookies[key];

            if (cookie != null)
            {
                if (httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(key, out string cookieValue))
                {
                    // Retrieve the cookie's expiration time
                    var expiration = httpContextAccessor.HttpContext.Request.Cookies[key];

                    // Parse the expiration date
                    if (DateTime.TryParse(expiration, out DateTime expirationDate))
                    {
                        if (expirationDate > DateTime.UtcNow)
                        {
                            // Cookie is not expired
                            return false;
                        }
                        else
                        {
                            // Cookie is expired
                            return true;
                        }
                    }
                    else
                    {
                        // Invalid expiration date format
                        return true;
                    }
                }
                else
                {
                    // Cookie not found
                    return true;
                }
            }
            else
            {
                // Cookie not found
                return true;
            }
        }
    }
}

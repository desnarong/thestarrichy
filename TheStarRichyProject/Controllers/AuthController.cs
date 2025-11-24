using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TheStarRichyProject.Helper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TheStarRichyProject.Controllers
{
    public class AuthController : BaseController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHtmlLocalizer<Localize.Resource> _localizer;
        IConfiguration _config;
        public AuthController(IHttpContextAccessor httpContextAccessor, ILogger<AuthController> logger, IHtmlLocalizer<Localize.Resource> localizer, ILoggerFactory loggerFactory, IConfiguration config) : base(httpContextAccessor, loggerFactory, config)
        {
            _config = config;
            _localizer = localizer;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Login(bool Timeout = false)
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> LoginAsync(string email, string password, string passkey = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            // Get Passkey from configuration if not provided
            passkey ??= _config["Api:Passkey"];
            if (string.IsNullOrEmpty(passkey))
            {
                return BadRequest(new { message = "Passkey is required" });
            }

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(_config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            // ข้ามการตรวจสอบใบรับรอง (สำหรับทดสอบเท่านั้น)
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                string ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString();

                // Create the JSON payload
                var payload = new
                {
                    username = email,
                    password,
                    passkey,
                    ipAddress
                };

                //
                var _client = new RestClient(options);

                // Create RestSharp request
                var request = new RestRequest("/Login/signin", Method.Post);
                request.AddJsonBody(payload);

                try
                {
                    // Make API call
                    var response = await _client.ExecuteAsync(request);

                    if (response.IsSuccessful)
                    {
                        var responseData = JsonConvert.DeserializeObject<dynamic>(response.Content);

                        // Extract JWT token
                        string token = responseData?.token?.ToString();
                        if (string.IsNullOrEmpty(token))
                        {
                            _logger.LogWarning("Login successful but no token returned");
                            return StatusCode(500, new { message = "Invalid response from server" });
                        }

                        request = new RestRequest("/Member/display", Method.Get);
                        request.AddHeader("X-Passkey", passkey);
                        request.AddHeader("Authorization", $"Bearer {token}");
                        request.AddHeader("Accept", "application/json");
                        response = await _client.ExecuteAsync(request);


                        var memberinfo = JsonConvert.DeserializeObject<dynamic>(response.Content);
                        string runMessage = memberinfo.runMessage != null ? (string)memberinfo.runMessage : "";
                        string systemName = memberinfo.systemname != null ? (string)memberinfo.systemname : "";
                        string language = memberinfo.lang != null ? (string)memberinfo.lang : "th";
                        string memberPicture = memberinfo.memberPositionPicture != null ? (string)memberinfo.memberPicture : "";
                        string memberCode = memberinfo.membercode != null ? (string)memberinfo.membercode : "";

                        ///Member/memberpermission
                        request = new RestRequest("/Member/memberpermission", Method.Get);
                        request.AddHeader("X-Passkey", passkey);
                        request.AddHeader("Authorization", $"Bearer {token}");
                        request.AddHeader("Accept", "application/json");
                        response = await _client.ExecuteAsync(request);
                        var memberpermission = JsonConvert.DeserializeObject<dynamic>(response.Content);

                        responseData.TotalBonus = memberinfo.totalBonus;

                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.MemberCodeKey, memberCode,
                           new CookieOptions
                           {
                               HttpOnly = true,
                               Secure = true,
                               SameSite = SameSiteMode.Strict,
                               Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"]))
                           }
                       );

                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.MemberPositionPictureKey, memberPicture,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"]))
                            }
                        );

                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.MessageInfoKey, runMessage,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"]))
                            }
                        );

                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.SystemInfoKey, systemName,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"]))
                            }
                        );

                        // Set JWT token in cookie
                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.UserKey, token,
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"])) // Match JWT expiration
                            }
                        );

                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.UserInfoKey, responseData.ToString(),
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"])) // Match JWT expiration
                            }
                        );

                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.MemberInfoKey, memberinfo.ToString(),
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"])) // Match JWT expiration
                            }
                        );

                        CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.PermissionsKey, JsonConvert.SerializeObject(memberpermission[0].ToString()),
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"])) // Match JWT expiration
                            }
                        );

                        //LanguageKey
                        //CookieHelper.SetCookie(_httpContextAccessor, CookieHelper.LanguageKey, language,
                        //    new CookieOptions
                        //    {
                        //        HttpOnly = true,
                        //        Secure = true,
                        //        SameSite = SameSiteMode.Strict,
                        //        Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(_config["Defualt:HourExpires"])) // Match JWT expiration
                        //    }
                        //);

                        return Ok(new
                        {
                            message = "Login successful",
                            redirectUrl = Url.Action("Index", "Home"),
                            lang = language
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Login failed: {StatusCode} - {ErrorBody}",
                            response.StatusCode,
                            response.Content);

                        return Unauthorized(new
                        {
                            message = "Invalid email or password"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login request");
                    return StatusCode(500, new { message = "An error occurred during login" });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during login");
                return StatusCode(503, new
                {
                    message = "Service unavailable. Please try again later."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred"
                });
            }
        }
        public IActionResult Logout()
        {
            //SessionModel config = SessionHelper.GetObjectFromJson<SessionModel>(HttpContext.Session, "user");
            //UsersSessionModel.ClearUserSession(config.User.Username);
            //SessionHelper.Remove(HttpContext.Session, "user");
            //SessionHelper.Remove(HttpContext.Session, "permissions");
            CookieHelper.ClearCookie(_httpContextAccessor, CookieHelper.UserKey);
            return Redirect("/Auth/Login");
        }
        public ActionResult Access()
        {
            return View();
        }
        public ActionResult Permission()
        {
            return View();
        }
        
    }
}

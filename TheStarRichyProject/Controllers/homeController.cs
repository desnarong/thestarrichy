using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TheStarRichyProject.Helper;
using TheStarRichyProject.Models;

namespace TheStarRichyProject.Controllers
{
    public class homeController : BaseController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<homeController> _logger;
        private readonly IConfiguration _config;

        public homeController(IHttpContextAccessor httpContextAccessor, ILogger<homeController> logger, ILoggerFactory loggerFactory, IConfiguration config) : base(httpContextAccessor, loggerFactory, config)
        {
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult index()
        {
            var cookieCheck = CheckCookie();
            if (cookieCheck != null)
            {
                return cookieCheck; // Redirect to login if cookie is invalid or expired
            }

            if (!Request.Cookies.ContainsKey(CookieHelper.UserKey))
            {
                return RedirectToAction("Index", "Login");
            }

            // Pass API URL to View
            ViewData["ApiMemberUrl"] = _config["Api:MemberUrl"];
            ViewData["Passkey"] = _config["Api:Passkey"];
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult GetToken()
        {
            var token = Request.Cookies[CookieHelper.UserKey];
            return Ok(new { token });
        }
        public async Task<IActionResult> GetMemberInfo()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/display", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer { token }");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberMessages()///Member/messages
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/messages", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberIncomeByPeriod()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/incomebyperiod", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberTeamBuyProduct()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/teambuyproduct", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberTeamByRegionBuy()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/teambyregionbuy", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberTeamByRegion()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/teambyregion", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberTeamNewBuy()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/teamnewbuy", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberTeamNewRegister()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/teamnewregister", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberTeamTotalPositionPackage()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/teamtotalpositionpackage", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetMemberTeamTotalPositionRanking()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/teamtotalpositionranking", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public async Task<IActionResult> GetIncomeByPeriod()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/incomebyperiod", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }
        public IActionResult GetSlideImages()
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/slidebar");
            if (!Directory.Exists(folderPath))
                return NotFound();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var files = Directory.GetFiles(folderPath)
                .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => Path.GetFileName(f))
                .ToList();

            return Ok(files);
        }
        public IActionResult GetPopupSlideImages()
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/popup");
            if (!Directory.Exists(folderPath))
                return NotFound();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var files = Directory.GetFiles(folderPath)
                .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => Path.GetFileName(f))
                .ToList();

            return Ok(files);
        }
        public async Task<IActionResult> GetMemberEstimatePosition()
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
            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];
            var client = new RestClient(options);
            var request = new RestRequest("/Member/estimateposition", Method.Get);
            request.AddHeader("X-Passkey", passkey);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                //Console.WriteLine(response.Content);
                return Ok(response.Content);
            }
            return Error();
        }

        [HttpGet]
        [Route("home/GetMemberBinaryTeam")]
        public async Task<IActionResult> GetMemberBinaryTeam(string? membercode = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var options = new RestClientOptions(_config["Api:Url"])
            {
                ThrowOnAnyError = true,
                MaxTimeout = 300000, // 5 นาที (เป็น milliseconds)
                ConfigureMessageHandler = handler =>
                {
                    var httpClientHandler = new HttpClientHandler
                    {
                        // ข้ามการตรวจสอบใบรับรอง (สำหรับทดสอบเท่านั้น)
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                        MaxResponseHeadersLength = 256, // เพิ่มขนาด headers
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                    };
                    return httpClientHandler;
                }
            };

            var passkey = _config["Api:Passkey"];
            var token = Request.Cookies[CookieHelper.UserKey];

            // ใช้ HttpClient โดยตรงแทน RestClient เพื่อควบคุมได้มากขึ้น
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_config["Api:Url"]);
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            httpClient.DefaultRequestHeaders.Add("X-Passkey", passkey);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var apiUrl = "/Member/memberbinaryteam";
            if (!string.IsNullOrEmpty(membercode))
            {
                apiUrl += $"?membercode={membercode}";
            }

            try
            {
                // ใช้ HttpCompletionOption.ResponseHeadersRead เพื่ออ่าน content ทีละส่วน
                var response = await httpClient.GetAsync(apiUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Content($"API Error: {response.StatusCode} - {errorContent}");
                }

                // อ่าน content เป็น string
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            catch (HttpRequestException ex)
            {
                return Content($"HttpRequestException: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                return Content($"Timeout: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Content($"Request Failed: {ex.Message} | Stack: {ex.StackTrace}");
            }
        }
    }
}

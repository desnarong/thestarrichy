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
    public class memberController : BaseController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<memberController> _logger;
        private readonly IConfiguration _config;
        public memberController(IHttpContextAccessor httpContextAccessor, ILogger<memberController> logger, ILoggerFactory loggerFactory, IConfiguration config) : base(httpContextAccessor, loggerFactory, config)
        {
            _config = config;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult index()
        {
            return View();
        }
        public IActionResult register()
        {
            return View();
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
        public async Task<IActionResult> GetBanks()
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
            var request = new RestRequest("/Static/banks", Method.Get);
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
        public async Task<IActionResult> GetCountries()
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
            var request = new RestRequest("/Static/countries", Method.Get);
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
        public async Task<IActionResult> GetCountryBusinesses()
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
            var request = new RestRequest("/Static/countrybusinesses", Method.Get);
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
        public async Task<IActionResult> GetDistricts()
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
            var request = new RestRequest("/Static/districts", Method.Get);
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
        public async Task<IActionResult> GetTitleNames()
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
            var request = new RestRequest("/Static/titlenames", Method.Get);
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

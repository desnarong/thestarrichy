using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System.Dynamic;
using System.Net;
using TheStarRichyProject.Helper;
using TheStarRichyProject.Models;

namespace TheStarRichyProject.Controllers
{
    public class ExternalRegistrationController : BaseController
    {
        private readonly ILogger<ExternalRegistrationController> _logger;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ExternalRegistrationController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration config,
            ILogger<ExternalRegistrationController> logger)
            : base(httpContextAccessor, loggerFactory, config)
        {
            _logger = logger;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Return the standalone external registration view (no layout)
            return View("~/Views/ExternalRegistration/Index.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPRequest request)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
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

                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Registration/SendOTP", Method.Post);
                apiRequest.AddStringBody(JsonConvert.SerializeObject(request), ContentType.Json);

                var response = await client.ExecuteAsync(apiRequest);

                if (response.IsSuccessful)
                {
                    var responseData = JsonConvert.DeserializeObject<SendOTPResponse>(response.Content);
                    return Ok(responseData);
                }
                else
                {
                    _logger.LogError("Failed to send OTP via API: {StatusCode} - {Content}", response.StatusCode, response.Content);
                    return StatusCode(500, new SendOTPResponse
                    {
                        Success = false,
                        Message = "เกิดข้อผิดพลาดในการส่ง OTP"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SendOTP API");
                return StatusCode(500, new SendOTPResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการส่ง OTP"
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
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

                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Registration/VerifyOTP", Method.Post);
                apiRequest.AddStringBody(JsonConvert.SerializeObject(request), ContentType.Json);

                var response = await client.ExecuteAsync(apiRequest);

                if (response.IsSuccessful)
                {
                    var responseData = JsonConvert.DeserializeObject<VerifyOTPResponse>(response.Content);
                    return Ok(responseData);
                }
                else
                {
                    _logger.LogError("Failed to verify OTP via API: {StatusCode} - {Content}", response.StatusCode, response.Content);
                    return StatusCode(500, new VerifyOTPResponse
                    {
                        Success = false,
                        Message = "เกิดข้อผิดพลาดในการตรวจสอบ OTP",
                        IsValid = false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling VerifyOTP API");
                return StatusCode(500, new VerifyOTPResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการตรวจสอบ OTP",
                    IsValid = false
                });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest("/Static/countries", Method.Get);
                AddHeaders(request);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return Ok(response.Content);
                }
                else
                {
                    return Ok(new { success = false, message = "ไม่สามารถโหลดข้อมูลประเทศได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetCountries API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการโหลดข้อมูลประเทศ" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountryBusinesses()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest("/Static/countrybusinesses", Method.Get);
                AddHeaders(request);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    //var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return Ok(response.Content);
                }
                else
                {
                    return Ok(new { success = false, message = "ไม่สามารถโหลดข้อมูลประเทศที่ดำเนินธุรกิจได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetCountryBusinesses API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการโหลดข้อมูลประเทศที่ดำเนินธุรกิจ" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FindReferrer(string referrerCode)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest($"/Registration/findreferrer?referrerCode={referrerCode}", Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var converter = new ExpandoObjectConverter();
                    dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(response.Content, converter);
                    return Ok(data);
                }
                else
                {
                    return Ok(new { success = false, message = "ไม่พบข้อมูลผู้แนะนำ" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling FindReferrer API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการค้นหาผู้แนะนำ" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTitles()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest("/Static/titles", Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return Ok(new { success = true, data = data });
                }
                else
                {
                    return Ok(new { success = false, message = "ไม่สามารถโหลดข้อมูลคำนำหน้าได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetTitles API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการโหลดข้อมูลคำนำหน้า" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupIDcardname(string name)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest($"/Registration/CheckDupIDcardname?idCardName={name}", Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    //var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return Ok(response.Content);
                }
                else
                {
                    return Ok(new { isDuplicate = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CheckDupIDcardname API");
                return Ok(new { isDuplicate = false });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupBusinessname(string name)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest($"/Registration/CheckDupBusinessname?businessName={name}", Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    //var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return Ok(response.Content);
                }
                else
                {
                    return Ok(new { isDuplicate = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CheckDupBusinessname API");
                return Ok(new { isDuplicate = false });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupIDcard(string id)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest($"/Registration/CheckDupIDcard?idCardNumber={id}", Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    //var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return Ok(response.Content);
                }
                else
                {
                    return Ok(new { isDuplicate = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CheckDupIDcard API");
                return Ok(new { isDuplicate = false });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupTelephone(string phone)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest($"/Registration/CheckDupTelephone?telephone={phone}", Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    //var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return Ok(response.Content);
                }
                else
                {
                    return Ok(new { isDuplicate = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CheckDupTelephone API");
                return Ok(new { isDuplicate = false });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupEmail(string email)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = true,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                var client = new RestClient(options);
                var request = new RestRequest($"/Registration/CheckDupEmail?email={email}", Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    //var data = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    return Ok(response.Content);
                }
                else
                {
                    return Ok(new { isDuplicate = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CheckDupEmail API");
                return Ok(new { isDuplicate = false });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> FinalizeRegistration([FromBody] dynamic request)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = false, // Changed to false to handle errors manually
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                string rawJsonString = request.GetRawText();

                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Registration/Finalize", Method.Post);
                apiRequest.AddStringBody(rawJsonString, "application/json");

                var response = await client.ExecuteAsync(apiRequest);

                if (response.IsSuccessful)
                {
                    return Ok(response.Content);
                }
                else
                {
                    _logger.LogError("FinalizeRegistration API failed: {StatusCode} - {Content}", response.StatusCode, response.Content);
                    return Ok(new { success = false, message = "ไม่สามารถลงทะเบียนได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling FinalizeRegistration API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการลงทะเบียน" });
            }
        }
        private void AddHeaders(RestRequest request)
        {
            var passkey = _config["Api:Passkey"];
            var token = _httpContextAccessor.HttpContext?.Request.Cookies[CookieHelper.UserKey];

            request.AddHeader("X-Passkey", passkey);

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            request.AddHeader("Accept", "application/json");
        }
    }
}

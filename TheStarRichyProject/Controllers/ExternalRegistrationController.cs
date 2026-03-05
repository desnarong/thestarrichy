using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System.Dynamic;
using System.Net;
using System.Text.Json.Nodes;
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
        public IActionResult GetClientIP()
        {
            try
            {
                // Get client IP address from request headers
                var ipAddress = GetClientIPAddress();
                
                return Ok(new 
                { 
                    success = true, 
                    ip = ipAddress,
                    message = "Client IP address retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client IP address");
                return Ok(new 
                { 
                    success = false, 
                    ip = "0",
                    message = "Could not detect client IP address"
                });
            }
        }

        private string GetClientIPAddress()
        {
            try
            {
                // Check for forwarded headers (when behind proxy/load balancer)
                var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For can contain multiple IPs, the first one is the client IP
                    var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (ips.Length > 0)
                    {
                        return ips[0].Trim();
                    }
                }

                // Check for other common proxy headers
                var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // Fall back to remote IP address
                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(remoteIp) && remoteIp != "::1")
                {
                    return remoteIp;
                }

                // Localhost or IPv6 localhost
                return "127.0.0.1";
            }
            catch
            {
                return "0";
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

                string ipAddress = GetClientIPAddress();

                // 1. ดึงข้อมูล JSON เดิมออกมาเป็น string
                string rawJsonString = request.GetRawText();

                // 2. แปลง string ให้เป็น JsonObject (เพื่อให้เพิ่ม/แก้ไขข้อมูลได้)
                var jsonObject = System.Text.Json.Nodes.JsonNode.Parse(rawJsonString).AsObject();

                // บังคับกรอกที่อยู่ตามบัตรให้ครบทุกช่อง
                var requiredAddressFields = new Dictionary<string, string>
                {
                    { "addressIdCard", "ที่อยู่ตามบัตร" },
                    { "postcode", "รหัสไปรษณีย์" },
                    { "provinceCode", "จังหวัด" },
                    { "districtCode", "เขต/อำเภอ" },
                    { "subdistrictCode", "แขวง/ตำบล" }
                };

                var missingAddressFields = requiredAddressFields
                    .Where(field => string.IsNullOrWhiteSpace(jsonObject[field.Key]?.ToString()))
                    .Select(field => field.Value)
                    .ToList();

                if (missingAddressFields.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"กรุณากรอกข้อมูลที่อยู่ตามบัตรให้ครบถ้วน: {string.Join(", ", missingAddressFields)}"
                    });
                }

                // ==========================================
                // 🌟 เพิ่มบล็อกจัดการบันทึกรูปภาพ (แปลง Base64 -> ไฟล์รูป)
                // ==========================================
                var memberPicArray = jsonObject["memberpic"]?.AsArray();
                if (memberPicArray != null && memberPicArray.Count > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Memberpicture");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var newPathsArray = new System.Text.Json.Nodes.JsonArray();
                    var index = 1;

                    // พยายามดึงเลขบัตรมาตั้งชื่อไฟล์ รองรับทั้ง documentNumber และ citizenNumber
                    string docNumber = jsonObject["documentNumber"]?.ToString()
                                       ?? jsonObject["citizenNumber"]?.ToString()
                                       ?? Guid.NewGuid().ToString("N").Substring(0, 13);

                    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                    foreach (var picNode in memberPicArray)
                    {
                        string base64String = picNode?.ToString();

                        // เช็คว่ามีข้อมูล Base64 จริงๆ
                        if (!string.IsNullOrWhiteSpace(base64String) && base64String.Length > 100)
                        {
                            try
                            {
                                // ตัดส่วน Header data:image/jpeg;base64, ทิ้งถ้ามี
                                var base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;
                                byte[] imageBytes = Convert.FromBase64String(base64Data);

                                // ตั้งชื่อไฟล์ Prefix ด้วย Reg_ (Registration)
                                string fileName = $"reg_{docNumber}_{timeStamp}_{index++}.jpg";
                                string filePath = Path.Combine(folderPath, fileName);

                                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                                // ✅ ตรวจสอบว่าไฟล์ถูกบันทึกสำเร็จ
                                if (!System.IO.File.Exists(filePath))
                                {
                                    _logger.LogWarning("File was not created successfully: {FilePath}", filePath);
                                    newPathsArray.Add((string)null);
                                    continue;
                                }

                                // ✅ ตรวจสอบว่าไฟล์มีขนาดมากกว่า 0 bytes
                                var fileInfo = new FileInfo(filePath);
                                if (fileInfo.Length == 0)
                                {
                                    _logger.LogWarning("File is empty (0 bytes): {FilePath}", filePath);
                                    System.IO.File.Delete(filePath); // ลบไฟล์ที่ว่างเปล่า
                                    newPathsArray.Add((string)null);
                                    continue;
                                }

                                // เก็บ Path ลง Array ใหม่
                                newPathsArray.Add($"/Images/Memberpicture/{fileName}");
                                
                                // Log สำหรับ debugging
                                _logger.LogInformation("Saved image: {FileName} for document {DocNumber}", fileName, docNumber);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to save member picture in FinalizeRegistration.");
                                newPathsArray.Add((string)null);
                            }
                        }
                        else
                        {
                            newPathsArray.Add((string)null);
                        }
                    }

                    // รูปภาพไม่บังคับ: ส่งเฉพาะไฟล์ที่บันทึกสำเร็จเท่านั้น
                    var validPaths = new System.Text.Json.Nodes.JsonArray();
                    foreach (var path in newPathsArray.Where(path => path != null && !string.IsNullOrWhiteSpace(path.ToString())))
                    {
                        validPaths.Add(path!.ToString());
                    }

                    // ใช้ "Memberpic" (ตัวใหญ่ M) เพื่อให้ตรงกับ property ใน FinalizeRegistrationRequest
                    jsonObject["Memberpic"] = validPaths;

                    _logger.LogInformation("Converted memberpic from Base64 to {Count} saved path(s)", validPaths.Count);
                }
                else
                {
                    // รองรับกรณีไม่แนบรูป: ส่งเป็น array ว่าง
                    jsonObject["Memberpic"] = new System.Text.Json.Nodes.JsonArray();
                    _logger.LogInformation("No member pictures uploaded; proceeding without images");
                }
                // ==========================================

                // 3. แทรก IpAddress เข้าไปใน object
                jsonObject["IpAddress"] = ipAddress;

                // 4. แปลงกลับเป็น JSON string ตัวใหม่ที่สมบูรณ์ (Base64 ถูกแทนที่ด้วย Path แล้ว)
                string finalJsonString = jsonObject.ToJsonString();

                // ส่ง finalJsonString ไปกับ RestClient
                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Registration/Finalize", Method.Post);
                apiRequest.AddStringBody(finalJsonString, "application/json");

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

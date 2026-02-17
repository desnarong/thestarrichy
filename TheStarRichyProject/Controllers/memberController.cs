using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
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
            var options = new RestClientOptions(_config["Api:Url"]!)
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
            var passkey = _config["Api:Passkey"]!;
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
            var options = new RestClientOptions(_config["Api:Url"]!)
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
            var passkey = _config["Api:Passkey"]!;
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
            var options = new RestClientOptions(_config["Api:Url"]!)
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
            var passkey = _config["Api:Passkey"]!;
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
            var options = new RestClientOptions(_config["Api:Url"]!)
            {
                ThrowOnAnyError = true,
                ConfigureMessageHandler = handler =>
                {
                    var httpClientHandler = new HttpClientHandler
                    {
                        // ������õ�Ǩ�ͺ��Ѻ�ͧ (����Ѻ���ͺ��ҹ��)
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                    return httpClientHandler;
                }
            };
            var passkey = _config["Api:Passkey"]!;
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
            var options = new RestClientOptions(_config["Api:Url"]!)
            {
                ThrowOnAnyError = true,
                ConfigureMessageHandler = handler =>
                {
                    var httpClientHandler = new HttpClientHandler
                    {
                        // ������õ�Ǩ�ͺ��Ѻ�ͧ (����Ѻ���ͺ��ҹ��)
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                    return httpClientHandler;
                }
            };
            var passkey = _config["Api:Passkey"]!;
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
            var options = new RestClientOptions(_config["Api:Url"]!)
            {
                ThrowOnAnyError = true,
                ConfigureMessageHandler = handler =>
                {
                    var httpClientHandler = new HttpClientHandler
                    {
                        // ������õ�Ǩ�ͺ��Ѻ�ͧ (����Ѻ���ͺ��ҹ��)
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                    return httpClientHandler;
                }
            };
            var passkey = _config["Api:Passkey"]!;
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
            var options = new RestClientOptions(_config["Api:Url"]!)
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
            var passkey = _config["Api:Passkey"]!;
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> FinalizeRegistration([FromBody] dynamic request)
        {
            try
            {
                var passkey = _config["Api:Passkey"]!;
                var token = Request.Cookies[CookieHelper.UserKey];
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = false,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                string ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString();

                // 1. ดึงข้อมูล JSON เดิมออกมาเป็น string
                string rawJsonString = request.GetRawText();

                // 2. แปลง string ให้เป็น JsonObject (เพื่อให้เพิ่ม/แก้ไขข้อมูลได้)
                var jsonObject = JsonNode.Parse(rawJsonString).AsObject();

                // ==========================================
                // 🌟 เพิ่มบล็อกจัดการบันทึกรูปภาพ (ถ้ามีส่งมา)
                // ==========================================
                var memberPicArray = jsonObject["memberpic"]?.AsArray();
                if (memberPicArray != null && memberPicArray.Count > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Memberpicture");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var newPathsArray = new JsonArray();
                    var index = 1;

                    // ฟอร์มแบบง่ายส่งมาเป็น documentNumber แทน citizenNumber
                    string docNumber = jsonObject["documentNumber"]?.ToString() ?? Guid.NewGuid().ToString("N").Substring(0, 13);
                    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                    foreach (var picNode in memberPicArray)
                    {
                        string base64String = picNode?.ToString();

                        // เช็คว่ามีข้อมูล Base64 จริงๆ
                        if (!string.IsNullOrWhiteSpace(base64String) && base64String.Length > 100)
                        {
                            try
                            {
                                var base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;
                                byte[] imageBytes = Convert.FromBase64String(base64Data);

                                // ตั้งชื่อไฟล์ เติม Prefix 'Easy_' เพื่อแยกแยะง่ายขึ้น
                                string fileName = $"Easy_{docNumber}_{timeStamp}_{index++}.jpg";
                                string filePath = Path.Combine(folderPath, fileName);

                                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                                newPathsArray.Add($"/Images/Memberpicture/{fileName}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to save member picture.");
                                newPathsArray.Add((string)null);
                            }
                        }
                        else
                        {
                            newPathsArray.Add((string)null);
                        }
                    }

                    // นำ Array Path ใหม่ ไปแทนที่ "memberpic" อันเก่า
                    jsonObject["memberpic"] = newPathsArray;
                }
                // ==========================================

                // 3. แทรก IpAddress เข้าไปใน object
                jsonObject["IpAddress"] = ipAddress;

                // 4. แปลงกลับเป็น JSON string ตัวใหม่ที่สมบูรณ์
                string finalJsonString = jsonObject.ToJsonString();

                // ส่ง finalJsonString ไปกับ RestClient
                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Registration/easyregistration", Method.Post);
                apiRequest.AddStringBody(finalJsonString, "application/json");
                apiRequest.AddHeader("X-Passkey", passkey);
                apiRequest.AddHeader("Authorization", $"Bearer {token}");
                apiRequest.AddHeader("Accept", "application/json");
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
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> FullFinalizeRegistration([FromBody] dynamic request)
        {
            try
            {
                var passkey = _config["Api:Passkey"]!;
                var token = Request.Cookies[CookieHelper.UserKey];
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(Config["Api:Url"])
                {
                    ThrowOnAnyError = false,
                    ConfigureMessageHandler = handler =>
                    {
                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        };
                        return httpClientHandler;
                    }
                };

                string ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString();

                // 1. ดึงข้อมูล JSON เดิมออกมาเป็น string และแปลงเป็น JsonObject
                string rawJsonString = request.GetRawText();
                var jsonObject = JsonNode.Parse(rawJsonString).AsObject();

                // 2. กำหนดโฟลเดอร์ที่จะเก็บรูป
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Memberpicture");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 3. จัดการเขียนรูปและแทนที่ Path ใน JSON
                var memberPicArray = jsonObject["memberpic"]?.AsArray();
                if (memberPicArray != null)
                {
                    var newPathsArray = new JsonArray(); // สร้าง Array ใหม่เพื่อเก็บ Path
                    var index = 1;

                    // ดึงเลขบัตร ปชช. มาใช้ตั้งชื่อไฟล์ (หรือใช้ GUID ถ้าไม่มี)
                    string citizenNumber = jsonObject["citizenNumber"]?.ToString() ?? Guid.NewGuid().ToString("N").Substring(0, 13);
                    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                    foreach (var picNode in memberPicArray)
                    {
                        string base64String = picNode?.ToString();

                        // เช็คว่ามีข้อมูล Base64 จริงๆ ไม่ใช่ค่าว่างหรือ null
                        if (!string.IsNullOrWhiteSpace(base64String) && base64String.Length > 100)
                        {
                            try
                            {
                                // แยกส่วนหัว "data:image/jpeg;base64," ออกถ้ามี
                                var base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;
                                byte[] imageBytes = Convert.FromBase64String(base64Data);

                                // ตั้งชื่อไฟล์ (ใช้ เลขบัตร_วันเวลา_ลำดับ.jpg เพื่อป้องกันชื่อซ้ำ)
                                string fileName = $"{citizenNumber}_{timeStamp}_{index++}.jpg";
                                string filePath = Path.Combine(folderPath, fileName);

                                // บันทึกไฟล์ลง Disk
                                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                                // เก็บ Path สั้นๆ ใส่เข้าไปใน Array ใหม่แทน Base64 เดิม
                                newPathsArray.Add($"/Images/Memberpicture/{fileName}");
                            }
                            catch (Exception ex)
                            {
                                // กรณี Base64 พังหรือไม่ใช่รูปภาพ
                                _logger.LogWarning(ex, "Failed to save member picture.");
                                newPathsArray.Add((string)null);
                            }
                        }
                        else
                        {
                            // ถ้าไม่ได้อัปโหลดรูปมา ให้ใส่ null ไว้ตามตำแหน่งเดิม
                            newPathsArray.Add((string)null);
                        }
                    }

                    // นำ Array Path ใหม่ ไปแทนที่ "memberpic" อันเก่าใน jsonObject
                    jsonObject["memberpic"] = newPathsArray;
                }

                // 4. แทรก IpAddress
                jsonObject["IpAddress"] = ipAddress;

                // 5. แปลงกลับเป็น JSON string ตัวใหม่ที่สมบูรณ์ (พร้อม Path รูปแทน Base64)
                string finalJsonString = jsonObject.ToJsonString();

                // 6. ส่ง finalJsonString ไปกับ RestClient
                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Registration/fullregistration", Method.Post);

                // ลบอันที่เบิ้ลออก เหลือแค่บรรทัดเดียว
                apiRequest.AddStringBody(finalJsonString, "application/json");

                apiRequest.AddHeader("X-Passkey", passkey);
                apiRequest.AddHeader("Authorization", $"Bearer {token}");
                apiRequest.AddHeader("Accept", "application/json");
                var response = await client.ExecuteAsync(apiRequest);

                if (response.IsSuccessful)
                {
                    return Ok(response.Content);
                }
                else
                {
                    _logger.LogError("FullFinalizeRegistration API failed: {StatusCode} - {Content}", response.StatusCode, response.Content);
                    return Ok(new { success = false, message = "ไม่สามารถลงทะเบียนได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling FullFinalizeRegistration API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการลงทะเบียน" });
            }
        }
    }
}

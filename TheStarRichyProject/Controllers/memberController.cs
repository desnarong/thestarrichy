using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

        [HttpPut]
        [HttpPost]
        [RequestSizeLimit(25_000_000)]
        public async Task<IActionResult> UpdateMemberProfile(
            [FromForm] string? payload,
            [FromForm] IFormFile? profileImage,
            [FromForm] IFormFile? copyIdCard,
            [FromForm] IFormFile? copyBankBook,
            [FromForm] IFormFile? applicationForm)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var savedUrls = new Dictionary<string, string?>();
                var hasFile = profileImage != null || copyIdCard != null || copyBankBook != null || applicationForm != null;

                JObject payloadObj;
                try { payloadObj = JObject.Parse(payload ?? "{}"); } catch { payloadObj = new JObject(); }

                if (hasFile)
                {
                    string memberCode = payloadObj["memberCode"]?.ToString() ?? "unknown";
                    string safeMemberCode = Regex.Replace(memberCode, "[^a-zA-Z0-9_-]", "");
                    string rootPath = Path.Combine(_config["Imagespath"], "Memberpicture", safeMemberCode);
                    if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

                    savedUrls["profileImage"] = await SaveUploadFileAsync(profileImage, rootPath, "profile");
                    savedUrls["copyIdCard"] = await SaveUploadFileAsync(copyIdCard, rootPath, "idcard");
                    savedUrls["copyBankBook"] = await SaveUploadFileAsync(copyBankBook, rootPath, "bankbook");
                    savedUrls["applicationForm"] = await SaveUploadFileAsync(applicationForm, rootPath, "application");

                    // อัปเดต documentInfo ใน payload ด้วย URL ที่ save ได้
                    var docInfo = payloadObj["documentInfo"] as JObject ?? new JObject();
                    if (savedUrls["profileImage"] != null) docInfo["profileImageUrl"] = savedUrls["profileImage"];
                    if (savedUrls["copyIdCard"] != null) docInfo["idCardImageUrl"] = savedUrls["copyIdCard"];
                    if (savedUrls["copyBankBook"] != null) docInfo["bankBookImageUrl"] = savedUrls["copyBankBook"];
                    if (savedUrls["applicationForm"] != null) docInfo["applicationFormImageUrl"] = savedUrls["applicationForm"];
                    payloadObj["documentInfo"] = docInfo;
                }

                var passkey = _config["Api:Passkey"]!;
                var token = Request.Cookies[CookieHelper.UserKey];
                var options = new RestClientOptions(_config["Api:Url"]!)
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
                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Member/profile", Method.Put);
                apiRequest.AddHeader("X-Passkey", passkey);
                apiRequest.AddHeader("Authorization", $"Bearer {token}");
                apiRequest.AddHeader("Accept", "application/json");
                apiRequest.AddStringBody(payloadObj.ToString(), "application/json");

                var response = await client.ExecuteAsync(apiRequest);
                if (!response.IsSuccessful)
                {
                    return StatusCode((int)(response.StatusCode == 0 ? HttpStatusCode.BadGateway : response.StatusCode), response.Content);
                }

                // ส่ง urls กลับไปให้ frontend อัปเดตรูป preview
                var result = JObject.Parse(response.Content ?? "{}");
                if (hasFile)
                {
                    result["urls"] = JObject.FromObject(new
                    {
                        profileImage = savedUrls.GetValueOrDefault("profileImage"),
                        copyIdCard = savedUrls.GetValueOrDefault("copyIdCard"),
                        copyBankBook = savedUrls.GetValueOrDefault("copyBankBook"),
                        applicationForm = savedUrls.GetValueOrDefault("applicationForm")
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling UpdateMemberProfile API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการอัปเดตข้อมูลสมาชิก" });
            }
        }

        [HttpPost]
        [RequestSizeLimit(25_000_000)]
        public async Task<IActionResult> UploadProfilePic4(
            [FromForm] string? memberCode,
            [FromForm] IFormFile? profileImage)
        {
            try
            {
                if (profileImage == null)
                {
                    return BadRequest(new { success = false, message = "กรุณาเลือกรูปโปรไฟล์" });
                }

                string safeMemberCode = string.IsNullOrWhiteSpace(memberCode)
                    ? "unknown"
                    : Regex.Replace(memberCode, "[^a-zA-Z0-9_-]", "");
                
                string rootPath = Path.Combine(_config["Imagespath"], "Memberpicture", safeMemberCode);
                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                string? profileImageUrl = await SaveUploadFileAsync(profileImage, rootPath, "profile");
                if (string.IsNullOrWhiteSpace(profileImageUrl))
                {
                    return StatusCode(500, new { success = false, message = "ไม่สามารถบันทึกรูปโปรไฟล์ได้" });
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var options = new RestClientOptions(_config["Api:Url"]!)
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

                var passkey = _config["Api:Passkey"]!;
                var token = Request.Cookies[CookieHelper.UserKey];
                var client = new RestClient(options);
                var apiRequest = new RestRequest("/Member/profile/pic4", Method.Put);
                apiRequest.AddHeader("X-Passkey", passkey);
                apiRequest.AddHeader("Authorization", $"Bearer {token}");
                apiRequest.AddHeader("Accept", "application/json");
                apiRequest.AddStringBody(JsonConvert.SerializeObject(new { profileImageUrl }), "application/json");

                var response = await client.ExecuteAsync(apiRequest);

                if (!response.IsSuccessful)
                {
                    return StatusCode((int)(response.StatusCode == 0 ? HttpStatusCode.BadGateway : response.StatusCode),
                        new { success = false, message = response.Content ?? "ไม่สามารถอัปเดตรูปโปรไฟล์ที่ระบบหลักได้" });
                }

                return Ok(new
                {
                    success = true,
                    url = profileImageUrl,
                    apiResponse = response.Content
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading member profile image and updating PIC4");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการอัปโหลดรูปโปรไฟล์" });
            }
        }

        [HttpPost]
        [RequestSizeLimit(25_000_000)]
        public async Task<IActionResult> UploadMemberDocuments(
            [FromForm] string? memberCode,
            [FromForm] IFormFile? profileImage,
            [FromForm] IFormFile? copyIdCard,
            [FromForm] IFormFile? copyBankBook,
            [FromForm] IFormFile? applicationForm)
        {
            try
            {
                if (profileImage == null && copyIdCard == null && copyBankBook == null && applicationForm == null)
                {
                    return BadRequest(new { success = false, message = "กรุณาเลือกไฟล์อย่างน้อย 1 รายการ" });
                }

                string safeMemberCode = string.IsNullOrWhiteSpace(memberCode)
                    ? "unknown"
                    : Regex.Replace(memberCode, "[^a-zA-Z0-9_-]", "");

                string rootPath = Path.Combine(_config["Imagespath"], "Memberpicture", safeMemberCode);
                
                if (!Directory.Exists(rootPath))
                {
                    Directory.CreateDirectory(rootPath);
                }

                var urls = new Dictionary<string, string?>
                {
                    ["profileImage"] = await SaveUploadFileAsync(profileImage, rootPath, "profile"),
                    ["copyIdCard"] = await SaveUploadFileAsync(copyIdCard, rootPath, "idcard"),
                    ["copyBankBook"] = await SaveUploadFileAsync(copyBankBook, rootPath, "bankbook"),
                    ["applicationForm"] = await SaveUploadFileAsync(applicationForm, rootPath, "application")
                };

                return Ok(new { success = true, urls });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading member documents");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการอัปโหลดเอกสาร" });
            }
        }

        private async Task<string?> SaveUploadFileAsync(IFormFile? file, string destinationFolder, string filePrefix)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(file.ContentType))
            {
                throw new InvalidOperationException("รองรับเฉพาะไฟล์รูปภาพประเภท JPG, PNG และ WEBP เท่านั้น");
            }

            string extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".jpg";
            }

            string filename = $"{filePrefix}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string fullPath = Path.Combine(destinationFolder, filename);

            // กำหนดขนาดสูงสุดที่ยอมรับได้ (300 KB)
            long maxFileSize = 300 * 1024;

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                if (file.Length > maxFileSize)
                {
                    // โหลดรูปภาพด้วย ImageSharp ถ้าขนาดเกิน 300KB
                    using var image = await Image.LoadAsync(file.OpenReadStream());

                    // 1. (ทางเลือก) ลดขนาดความกว้าง/สูง หากภาพใหญ่เกินไป 
                    // เพราะการลด Quality อย่างเดียวอาจจะไม่ทำให้ไฟล์เล็กลงถึง 300KB ได้ถ้ารูปมีขนาดใหญ่มาก (เช่น 4K)
                    int maxWidth = 1200; // กำหนดความกว้างสูงสุดที่ต้องการ
                    if (image.Width > maxWidth)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(maxWidth, 0), // 0 คือให้คำนวณ Height อัตโนมัติตามสัดส่วน
                            Mode = ResizeMode.Max
                        }));
                    }

                    // 2. ปรับลด Quality ตามประเภทของไฟล์ภาพ
                    if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        var encoder = new JpegEncoder { Quality = 75 }; // ปรับค่า 1-100 (แนะนำ 70-80)
                        await image.SaveAsync(stream, encoder);
                    }
                    else if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
                    {
                        var encoder = new WebpEncoder { Quality = 75 };
                        await image.SaveAsync(stream, encoder);
                    }
                    else
                    {
                        // สำหรับ PNG เป็น Lossless (ไม่สูญเสียความละเอียด) การลดขนาดไฟล์โดยไม่ลด Dimension ทำได้ยาก
                        // โค้ดนี้จะใช้ไฟล์ที่ถูกลด Dimension (ถ้ามี) แล้วเซฟทับไปเป็น format เดิม
                        await image.SaveAsync(stream, image.Metadata.DecodedImageFormat);
                    }
                }
                else
                {
                    // ถ้าขนาดไม่เกิน 300KB บันทึกไฟล์ต้นฉบับได้เลย ไม่ต้องผ่านกระบวนการแปลงภาพ
                    await file.CopyToAsync(stream);
                }
            }

            string relativeFolder = destinationFolder
                .Replace(_config["Imagespath"], "Images")
                .Replace("\\", "/")
                .TrimStart('/');

            return $"{relativeFolder}/{filename}";
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

                string ipAddress = GetClientIPAddress();

                // 1. ดึงข้อมูล JSON เดิมออกมาเป็น string
                //string rawJsonString = request.GetRawText();
                string rawJsonString = request is System.Text.Json.JsonElement je
                    ? je.GetRawText()
                    : Newtonsoft.Json.JsonConvert.SerializeObject(request);

                // 2. แปลง string ให้เป็น JsonObject (เพื่อให้เพิ่ม/แก้ไขข้อมูลได้)
                var jsonObject = JsonNode.Parse(rawJsonString).AsObject();

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

                                // ตั้งชื่อไฟล์ ใช้รูปแบบ reg_ เพื่อให้เหมือนกันทั้งหมด
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

                                newPathsArray.Add($"Images/Memberpicture/{fileName}");
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

                    // รูปภาพไม่บังคับ: ส่งเฉพาะไฟล์ที่บันทึกสำเร็จเท่านั้น
                    var validPaths = new JsonArray();
                    foreach (var path in newPathsArray.Where(path => path != null && !string.IsNullOrWhiteSpace(path.ToString())))
                    {
                        validPaths.Add(path!.ToString());
                    }

                    jsonObject["memberpic"] = validPaths;
                }
                else
                {
                    jsonObject["memberpic"] = new JsonArray();
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

                string ipAddress = GetClientIPAddress();

                // 1. ดึงข้อมูล JSON เดิมออกมาเป็น string และแปลงเป็น JsonObject
                //string rawJsonString = request.GetRawText();
                //var jsonObject = JsonNode.Parse(rawJsonString).AsObject();

                string rawJsonString = request is System.Text.Json.JsonElement je
                    ? je.GetRawText()
                    : Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var jsonObject = JsonNode.Parse(rawJsonString).AsObject();

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

                    // ใช้ documentNumber แทน citizenNumber เพื่อให้รูปแบบชื่อไฟล์เหมือนกันทั้งหมด
                    string docNumber = jsonObject["documentNumber"]?.ToString() 
                                       ?? jsonObject["citizenNumber"]?.ToString()
                                       ?? Guid.NewGuid().ToString("N").Substring(0, 13);
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

                                // ตั้งชื่อไฟล์ ใช้รูปแบบ reg_ เพื่อให้เหมือนกันทั้งหมด
                                string fileName = $"reg_{docNumber}_{timeStamp}_{index++}.jpg";
                                string filePath = Path.Combine(folderPath, fileName);

                                // บันทึกไฟล์ลง Disk
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

                                // เก็บ Path สั้นๆ ใส่เข้าไปใน Array ใหม่แทน Base64 เดิม
                                newPathsArray.Add($"Images/Memberpicture/{fileName}");
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

                    // รูปภาพไม่บังคับ: ส่งเฉพาะไฟล์ที่บันทึกสำเร็จเท่านั้น
                    var validPaths = new JsonArray();
                    foreach (var path in newPathsArray.Where(path => path != null && !string.IsNullOrWhiteSpace(path.ToString())))
                    {
                        validPaths.Add(path!.ToString());
                    }

                    jsonObject["memberpic"] = validPaths;
                }
                else
                {
                    jsonObject["memberpic"] = new JsonArray();
                }

                // 4. แทรก IpAddress (ใช้ ipAddress ที่ได้จาก function)
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

        // ============================================================
        // Profile Addresses — GET (proxy → /Member/profile-addresses)
        // ============================================================
        public async Task<IActionResult> GetProfileAddresses()
        {
            try
            {
                var options = new RestClientOptions(_config["Api:Url"]!)
                {
                    ThrowOnAnyError = false,
                    ConfigureMessageHandler = _ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    }
                };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var passkey = _config["Api:Passkey"]!;
                var token   = Request.Cookies[CookieHelper.UserKey];
                var client  = new RestClient(options);
                var req     = new RestRequest("/Member/profile-addresses", Method.Get);
                req.AddHeader("X-Passkey",     passkey);
                req.AddHeader("Authorization", $"Bearer {token}");
                req.AddHeader("Accept",        "application/json");

                var response = await client.ExecuteAsync(req);
                if (response.IsSuccessful)
                    return Ok(response.Content);

                return StatusCode((int)(response.StatusCode == 0 ? HttpStatusCode.BadGateway : response.StatusCode),
                    response.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GetProfileAddresses API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการดึงข้อมูลที่อยู่" });
            }
        }

        // ============================================================
        // Profile Addresses — PUT (proxy → /Member/profile-addresses)
        // ============================================================
        [HttpPut]
        public async Task<IActionResult> UpdateProfileAddresses([FromBody] JObject requestBody)
        {
            try
            {
                var options = new RestClientOptions(_config["Api:Url"]!)
                {
                    ThrowOnAnyError = false,
                    ConfigureMessageHandler = _ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    }
                };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var passkey = _config["Api:Passkey"]!;
                var token   = Request.Cookies[CookieHelper.UserKey];
                var client  = new RestClient(options);
                var req     = new RestRequest("/Member/profile-addresses", Method.Put);
                req.AddHeader("X-Passkey",     passkey);
                req.AddHeader("Authorization", $"Bearer {token}");
                req.AddHeader("Accept",        "application/json");
                req.AddStringBody(requestBody?.ToString() ?? "{}", "application/json");

                var response = await client.ExecuteAsync(req);
                if (response.IsSuccessful)
                    return Ok(response.Content);

                return StatusCode((int)(response.StatusCode == 0 ? HttpStatusCode.BadGateway : response.StatusCode),
                    response.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling UpdateProfileAddresses API");
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการบันทึกที่อยู่" });
            }
        }
    }
}

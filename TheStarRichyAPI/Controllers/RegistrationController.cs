using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheStarRichyApi.Models;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            IRegistrationService registrationService,
            ILogger<RegistrationController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        /// <summary>
        /// ลงทะเบียนสมาชิกแบบง่าย (Easy Registration) - ต้อง login
        /// POST /Registration/easyregistration
        /// </summary>
        [HttpPost("easyregistration")]
        [Authorize]
        public async Task<IActionResult> EasyRegistration([FromBody] EasyRegistrationRequest request)
        {
            try
            {
                var missingAddressFields = new List<string>();
                if (string.IsNullOrWhiteSpace(request.AddressIdCard)) missingAddressFields.Add("ที่อยู่ตามบัตร");
                if (string.IsNullOrWhiteSpace(request.Postcode)) missingAddressFields.Add("รหัสไปรษณีย์");
                if (string.IsNullOrWhiteSpace(request.ProvinceCode)) missingAddressFields.Add("จังหวัด");
                if (string.IsNullOrWhiteSpace(request.DistrictCode)) missingAddressFields.Add("เขต/อำเภอ");
                if (string.IsNullOrWhiteSpace(request.SubdistrictCode)) missingAddressFields.Add("แขวง/ตำบล");

                if (missingAddressFields.Any())
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = $"กรุณากรอกข้อมูลที่อยู่ตามบัตรให้ครบถ้วน: {string.Join(", ", missingAddressFields)}"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ครบถ้วนหรือไม่ถูกต้อง",
                        Errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid"
                        )
                    });
                }

                // ตรวจสอบเลขบัตรประชาชน/เอกสารซ้ำ
                if (await _registrationService.IsDocumentNumberExistsAsync(request.DocumentNumber))
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = "เลขที่เอกสารนี้ถูกใช้ลงทะเบียนแล้ว"
                    });
                }

                // Get current member code from JWT token
                var currentMemberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                

                var result = await _registrationService.EasyRegisterAsync(request, currentMemberCode);

                if (result.Success)
                {
                    _logger.LogInformation("Easy registration successful for {DocumentNumber} by {MemberCode}", 
                        request.DocumentNumber, currentMemberCode);
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in easy registration endpoint");
                return StatusCode(500, new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดภายในระบบ"
                });
            }
        }

        /// <summary>
        /// ลงทะเบียนสมาชิกแบบเต็ม (Full Registration) - ต้อง login
        /// POST /Registration/fullregistration
        /// </summary>
        [HttpPost("fullregistration")]
        [Authorize]
        public async Task<IActionResult> FullRegistration([FromBody] FullRegistrationRequest request)
        {
            try
            {
                var missingAddressFields = new List<string>();
                if (string.IsNullOrWhiteSpace(request.AddressIdCard)) missingAddressFields.Add("ที่อยู่ตามบัตร");
                if (string.IsNullOrWhiteSpace(request.Postcode)) missingAddressFields.Add("รหัสไปรษณีย์");
                if (string.IsNullOrWhiteSpace(request.ProvinceCode)) missingAddressFields.Add("จังหวัด");
                if (string.IsNullOrWhiteSpace(request.DistrictCode)) missingAddressFields.Add("เขต/อำเภอ");
                if (string.IsNullOrWhiteSpace(request.SubdistrictCode)) missingAddressFields.Add("แขวง/ตำบล");

                if (missingAddressFields.Any())
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = $"กรุณากรอกข้อมูลที่อยู่ตามบัตรให้ครบถ้วน: {string.Join(", ", missingAddressFields)}"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ครบถ้วนหรือไม่ถูกต้อง",
                        Errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid"
                        )
                    });
                }

                // ตรวจสอบเลขบัตรประชาชน/เอกสารซ้ำ
                if (await _registrationService.IsDocumentNumberExistsAsync(request.DocumentNumber))
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = "เลขที่เอกสารนี้ถูกใช้ลงทะเบียนแล้ว"
                    });
                }

                // Get current member code from JWT token
                var currentMemberCode = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                

                var result = await _registrationService.FullRegisterAsync(request, currentMemberCode);

                if (result.Success)
                {
                    _logger.LogInformation("Full registration successful for {DocumentNumber} by {MemberCode}", 
                        request.DocumentNumber, currentMemberCode);
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in full registration endpoint");
                return StatusCode(500, new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดภายในระบบ"
                });
            }
        }

        /// <summary>
        /// ลงทะเบียนสมาชิกจากภายนอก (External Registration) - ไม่ต้อง login
        /// POST /Registration/externalregistration
        /// </summary>
        [HttpPost("externalregistration")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalRegistration([FromBody] ExternalRegistrationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ครบถ้วนหรือไม่ถูกต้อง",
                        Errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid"
                        )
                    });
                }

                // ตรวจสอบเลขบัตรประชาชน/เอกสารซ้ำ
                if (await _registrationService.IsDocumentNumberExistsAsync(request.DocumentNumber))
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = "เลขที่เอกสารนี้ถูกใช้ลงทะเบียนแล้ว"
                    });
                }

                var result = await _registrationService.ExternalRegisterAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("External registration successful for {DocumentNumber} from {Source}", 
                        request.DocumentNumber, request.SourcePage);
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in external registration endpoint");
                return StatusCode(500, new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดภายในระบบ"
                });
            }
        }

        /// <summary>
        /// ค้นหาข้อมูลผู้อ้างอิง
        /// GET /Registration/findreferrer?referrerCode=xxx
        /// </summary>
        [HttpGet("findreferrer")]
        [AllowAnonymous]
        public async Task<IActionResult> FindReferrer([FromQuery] string referrerCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(referrerCode))
                {
                    return BadRequest(new FindReferrerResponse
                    {
                        Success = false,
                        Message = "กรุณาระบุรหัสผู้อ้างอิง"
                    });
                }

                var result = await _registrationService.FindReferrerAsync(referrerCode);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding referrer {ReferrerCode}", referrerCode);
                return StatusCode(500, new FindReferrerResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการค้นหาข้อมูล"
                });
            }
        }

        /// <summary>
        /// ตรวจสอบว่าเลขบัตรประชาชน/เอกสารซ้ำหรือไม่
        /// GET /Registration/checkdocument?documentNumber=xxx
        /// </summary>
        [HttpGet("checkdocument")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDocumentNumber([FromQuery] string documentNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentNumber))
                {
                    return BadRequest(new { success = false, message = "กรุณาระบุเลขที่เอกสาร" });
                }

                var exists = await _registrationService.IsDocumentNumberExistsAsync(documentNumber);
                
                return Ok(new 
                { 
                    success = true, 
                    exists = exists,
                    message = exists ? "เลขที่เอกสารนี้ถูกใช้ลงทะเบียนแล้ว" : "เลขที่เอกสารนี้สามารถใช้ได้"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking document number {DocumentNumber}", documentNumber);
                return StatusCode(500, new { success = false, message = "เกิดข้อผิดพลาดในการตรวจสอบข้อมูล" });
            }
        }

        #region Validation Endpoints for External Registration

        /// <summary>
        /// ตรวจสอบบัญชีดำ (Blacklist Check)
        /// GET /Registration/CheckBlacklist?idCardNumber={value}
        /// </summary>
        [HttpGet("CheckBlacklist")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckBlacklist([FromQuery] string idCardNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(idCardNumber))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกเลขบัตรประชาชน" });

                var result = await _registrationService.CheckBlacklistAsync(idCardNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckBlacklist endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบบัตรหมดอายุ (Expire Check)
        /// GET /Registration/CheckExpire?idCardNumber={value}
        /// </summary>
        [HttpGet("CheckExpire")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckExpire([FromQuery] string idCardNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(idCardNumber))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกเลขบัตรประชาชน" });

                var result = await _registrationService.CheckExpireAsync(idCardNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckExpire endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบสมาชิกลาออก (Member Resign Check)
        /// GET /Registration/CheckMemberResign?idCardNumber={value}
        /// </summary>
        [HttpGet("CheckMemberResign")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckMemberResign([FromQuery] string idCardNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(idCardNumber))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกเลขบัตรประชาชน" });

                var result = await _registrationService.CheckMemberResignAsync(idCardNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckMemberResign endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบรหัสผู้แนะนำ (Sponsor Code Check)
        /// GET /Registration/CheckSponsorCode?memberCode={value}
        /// </summary>
        [HttpGet("CheckSponsorCode")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckSponsorCode([FromQuery] string memberCode)
        {
            try
            {
                if (string.IsNullOrEmpty(memberCode))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกรหัสผู้แนะนำ" });

                var result = await _registrationService.CheckSponsorCodeAsync(memberCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckSponsorCode endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบเลขบัตรซ้ำ (Duplicate ID Card Check)
        /// GET /Registration/CheckDupIDcard?idCardNumber={value}
        /// </summary>
        [HttpGet("CheckDupIDcard")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupIDcard([FromQuery] string idCardNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(idCardNumber))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกเลขบัตรประชาชน" });

                var result = await _registrationService.CheckDuplicateIDCardAsync(idCardNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupIDcard endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบชื่อตามบัตรซ้ำ (Duplicate ID Card Name Check)
        /// GET /Registration/CheckDupIDcardname?idCardName={value}
        /// </summary>
        [HttpGet("CheckDupIDcardname")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupIDcardname([FromQuery] string idCardName)
        {
            try
            {
                if (string.IsNullOrEmpty(idCardName))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกชื่อตามบัตรประชาชน" });

                var result = await _registrationService.CheckDuplicateIDCardNameAsync(idCardName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupIDcardname endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบชื่อธุรกิจซ้ำ (Duplicate Business Name Check)
        /// GET /Registration/CheckDupBusinessname?businessName={value}
        /// </summary>
        [HttpGet("CheckDupBusinessname")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupBusinessname([FromQuery] string businessName)
        {
            try
            {
                if (string.IsNullOrEmpty(businessName))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกชื่อธุรกิจ" });

                var result = await _registrationService.CheckDuplicateBusinessNameAsync(businessName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupBusinessname endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบเบอร์โทรซ้ำ (Duplicate Telephone Check)
        /// GET /Registration/CheckDupTelephone?telephone={value}
        /// </summary>
        [HttpGet("CheckDupTelephone")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupTelephone([FromQuery] string telephone)
        {
            try
            {
                if (string.IsNullOrEmpty(telephone))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกเบอร์โทรศัพท์" });

                var result = await _registrationService.CheckDuplicateTelephoneAsync(telephone);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupTelephone endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบเลขบัญชีธนาคารซ้ำ (Duplicate Bank Account Check)
        /// GET /Registration/CheckDupBankAccountNumber?bankCode={value}&accountNumber={value}
        /// </summary>
        [HttpGet("CheckDupBankAccountNumber")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupBankAccountNumber([FromQuery] string bankCode, [FromQuery] string accountNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(accountNumber))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกข้อมูลบัญชีธนาคารให้ครบถ้วน" });

                var result = await _registrationService.CheckDuplicateBankAccountAsync(bankCode, accountNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupBankAccountNumber endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบชื่อบัญชีธนาคารซ้ำ (Duplicate Bank Account Name Check)
        /// GET /Registration/CheckDupBankAccountName?bankCode={value}&accountName={value}
        /// </summary>
        [HttpGet("CheckDupBankAccountName")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupBankAccountName([FromQuery] string bankCode, [FromQuery] string accountName)
        {
            try
            {
                if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(accountName))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกข้อมูลบัญชีธนาคารให้ครบถ้วน" });

                var result = await _registrationService.CheckDuplicateBankAccountNameAsync(bankCode, accountName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupBankAccountName endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบอีเมลซ้ำ (Duplicate Email Check)
        /// GET /Registration/CheckDupEmail?email={value}
        /// </summary>
        [HttpGet("CheckDupEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกอีเมล" });

                var result = await _registrationService.CheckDuplicateEmailAsync(email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupEmail endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบ Line ID ซ้ำ (Duplicate Line ID Check)
        /// GET /Registration/CheckDupLineid?lineId={value}
        /// </summary>
        [HttpGet("CheckDupLineid")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDupLineid([FromQuery] string lineId)
        {
            try
            {
                if (string.IsNullOrEmpty(lineId))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอก Line ID" });

                var result = await _registrationService.CheckDuplicateLineIdAsync(lineId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDupLineid endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        /// <summary>
        /// ตรวจสอบอายุ (Age Check)
        /// GET /Registration/CheckAge?birthDate={value}
        /// </summary>
        [HttpGet("CheckAge")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAge([FromQuery] string birthDate)
        {
            try
            {
                if (string.IsNullOrEmpty(birthDate))
                    return BadRequest(new ValidationResponse { Success = false, Message = "กรุณากรอกวันเกิด" });

                var result = await _registrationService.CheckAgeAsync(birthDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAge endpoint");
                return StatusCode(500, new ValidationResponse { Success = false, Message = "เกิดข้อผิดพลาดภายในระบบ" });
            }
        }

        #endregion

        #region Client IP Endpoint

        /// <summary>
        /// Get client IP address from server context
        /// GET /Registration/GetClientIP
        /// </summary>
        [HttpGet("GetClientIP")]
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
                        var clientIp = ips[0].Trim();
                        // Convert IPv6 to IPv4 if possible
                        return NormalizeIPAddress(clientIp);
                    }
                }

                // Check for other common proxy headers
                var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return NormalizeIPAddress(realIp);
                }

                // Fall back to remote IP address
                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(remoteIp) && remoteIp != "::1")
                {
                    return NormalizeIPAddress(remoteIp);
                }

                // Localhost or IPv6 localhost
                return "127.0.0.1";
            }
            catch
            {
                return "0";
            }
        }

        private string NormalizeIPAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "0";

            // If it's IPv6 localhost, return IPv4 localhost
            if (ipAddress == "::1" || ipAddress == "0:0:0:0:0:0:0:1")
                return "127.0.0.1";

            // If it's IPv6-mapped IPv4 address (::ffff:192.168.1.1), extract IPv4
            if (ipAddress.StartsWith("::ffff:"))
            {
                var ipv4Part = ipAddress.Substring(7);
                if (System.Net.IPAddress.TryParse(ipv4Part, out var ipv4Addr))
                {
                    return ipv4Addr.ToString();
                }
            }

            // If it's a pure IPv6 address, try to convert to IPv4 if possible
            if (System.Net.IPAddress.TryParse(ipAddress, out var ipAddr))
            {
                // If it's IPv4, return as is
                if (ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ipAddr.ToString();
                }
                
                // If it's IPv6, check if it's an IPv4-mapped IPv6 address
                if (ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    // Check if it's an IPv4-mapped IPv6 address
                    if (ipAddr.IsIPv4MappedToIPv6)
                    {
                        return ipAddr.MapToIPv4().ToString();
                    }
                    
                    // For other IPv6 addresses, return the original (or could return "0" if you only want IPv4)
                    // For now, return the original IPv6
                    return ipAddr.ToString();
                }
            }

            // Return original if parsing failed
            return ipAddress;
        }

        #endregion

        #region OTP Endpoints

        /// <summary>
        /// ส่ง OTP ไปยังเบอร์โทรศัพท์
        /// POST /Registration/SendOTP
        /// </summary>
        [HttpPost("SendOTP")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new SendOTPResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ครบถ้วนหรือไม่ถูกต้อง"
                    });
                }

                var result = await _registrationService.SendOTPAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendOTP endpoint");
                return StatusCode(500, new SendOTPResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการส่ง OTP"
                });
            }
        }

        /// <summary>
        /// ตรวจสอบ OTP
        /// POST /Registration/VerifyOTP
        /// </summary>
        [HttpPost("VerifyOTP")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new VerifyOTPResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ครบถ้วนหรือไม่ถูกต้อง"
                    });
                }

                var result = await _registrationService.VerifyOTPAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyOTP endpoint");
                return StatusCode(500, new VerifyOTPResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดในการตรวจสอบ OTP"
                });
            }
        }

        #endregion

        #region Final Registration

        /// <summary>
        /// ลงทะเบียนสมาชิกพร้อม OTP verification (External Registration with OTP)
        /// POST /Registration/Finalize
        /// </summary>
        [HttpPost("Finalize")]
        [AllowAnonymous]
        public async Task<IActionResult> FinalizeRegistration([FromBody] FinalizeRegistrationRequest request)
        {
            try
            {
                // บังคับกรอกที่อยู่ตามบัตรให้ครบทุกช่อง
                var missingAddressFields = new List<string>();
                if (string.IsNullOrWhiteSpace(request.AddressIdCard)) missingAddressFields.Add("ที่อยู่ตามบัตร");
                if (string.IsNullOrWhiteSpace(request.Postcode)) missingAddressFields.Add("รหัสไปรษณีย์");
                if (string.IsNullOrWhiteSpace(request.ProvinceCode)) missingAddressFields.Add("จังหวัด");
                if (string.IsNullOrWhiteSpace(request.DistrictCode)) missingAddressFields.Add("เขต/อำเภอ");
                if (string.IsNullOrWhiteSpace(request.SubdistrictCode)) missingAddressFields.Add("แขวง/ตำบล");

                if (missingAddressFields.Any())
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = $"กรุณากรอกข้อมูลที่อยู่ตามบัตรให้ครบถ้วน: {string.Join(", ", missingAddressFields)}"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new RegistrationResponse
                    {
                        Success = false,
                        Message = "ข้อมูลไม่ครบถ้วนหรือไม่ถูกต้อง",
                        Errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid"
                        )
                    });
                }

                var result = await _registrationService.FinalizeRegistrationAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("Final registration successful for {DocumentNumber}", 
                        request.DocumentNumber);
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FinalizeRegistration endpoint");
                return StatusCode(500, new RegistrationResponse
                {
                    Success = false,
                    Message = "เกิดข้อผิดพลาดภายในระบบ"
                });
            }
        }

        #endregion
    }
}

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
                if (await _registrationService.IsDocumentNumberExistsAsync(request.CitizenNumber))
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
                    _logger.LogInformation("Full registration successful for {CitizenNumber} by {MemberCode}", 
                        request.CitizenNumber, currentMemberCode);
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
    }
}

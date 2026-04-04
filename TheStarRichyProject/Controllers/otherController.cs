using Microsoft.AspNetCore.Mvc;
using TheStarRichyProject.Services;

namespace TheStarRichyProject.Controllers
{
    public class otherController : Controller
    {
        private readonly IApiService _apiService;

        public otherController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult changepassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckOldPassword([FromBody] CheckPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.OldPassword))
                return Ok(new { success = false, message = "กรุณากรอกรหัสผ่านเก่า" });
            try
            {
                var result = await _apiService.PostAsync<dynamic>("/Member/checkoldpassword", new { OldPassword = request.OldPassword });
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "ไม่สามารถตรวจสอบรหัสผ่านได้" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.NewPassword))
                return Ok(new { success = false, message = "กรุณากรอกรหัสผ่านใหม่" });
            try
            {
                var result = await _apiService.PostAsync<dynamic>("/Member/changepassword", new { NewPassword = request.NewPassword });
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "ไม่สามารถเปลี่ยนรหัสผ่านได้" });
            }
        }
        public IActionResult taxdownload()
        {
            return View();
        }
        public IActionResult documents()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTaxInvoiceData(string? fromDate, string? toDate)
        {
            try
            {
                var endpoint = $"/Member/reporttaxinvoice?fromDate={fromDate}&toDate={toDate}";
                var result = await _apiService.GetAsync<dynamic>(endpoint);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลใบกำกับภาษีได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTaxInvoiceDetailData(string? billNo)
        {
            if (string.IsNullOrWhiteSpace(billNo))
                return Ok(new List<object>());

            try
            {
                var endpoint = $"/Member/reporttaxinvoicedetail?billNo={Uri.EscapeDataString(billNo)}";
                var result = await _apiService.GetAsync<dynamic>(endpoint);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลรายละเอียดใบกำกับภาษีได้" });
            }
        }
    }
    public class CheckPasswordRequest { public string? OldPassword { get; set; } }
    public class ChangePasswordRequest { public string? NewPassword { get; set; } }}

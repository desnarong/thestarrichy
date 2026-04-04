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
}

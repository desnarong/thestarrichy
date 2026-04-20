using Microsoft.AspNetCore.Mvc;
using TheStarRichyProject.Services;

namespace TheStarRichyProject.Controllers
{
    [Route("[controller]/[action]")]
    public class buyordersController : Controller
    {
        private readonly IApiService _apiService;

        public buyordersController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult buyorderbyewallet()
        {
            return View();
        }

        public IActionResult saleorderfromhold()
        {
            return View();
        }

        public IActionResult ewallettransfer()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSaleOrderFromHold(string membercode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(membercode))
                    return BadRequest(new { success = false, message = "กรุณาระบุรหัสสมาชิก" });

                var result = await _apiService.GetAsync<dynamic>($"/Member/getsaleorderfromhold?membercode={Uri.EscapeDataString(membercode)}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindMemberForSale(string membercode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(membercode))
                    return Ok(new List<object>());

                var result = await _apiService.GetAsync<dynamic>($"/Product/findmembercodeforsale?memberCode={Uri.EscapeDataString(membercode)}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

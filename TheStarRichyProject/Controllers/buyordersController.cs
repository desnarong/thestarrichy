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
        public async Task<IActionResult> GetSaleOrderFromHold(string purchaseType, string productCode, string productName, string quantity)
        {
            try
            {
                // เรียก API ตามประเภทการซื้อ
                var endpoint = purchaseType == "hurry"
                    ? "/Product/GetProductListForHurry"
                    : "/Product/GetProductListForHold";

                // เรียก API ด้วย parameters
                var queryString = $"?groupcode=&producttype=&sortorder=&productid={productCode}";
                var result = await _apiService.GetAsync<dynamic>(endpoint + queryString);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

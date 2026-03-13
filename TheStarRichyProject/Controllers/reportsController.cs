using Microsoft.AspNetCore.Mvc;
using TheStarRichyProject.Services;

namespace TheStarRichyProject.Controllers
{
    public class reportsController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<reportsController> _logger;

        public reportsController(IApiService apiService, ILogger<reportsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public IActionResult income()
        {
            return View();
        }
        public IActionResult pointhistory()
        {
            return View();
        }
        public IActionResult orderhistory()
        {
            return View();
        }
        public IActionResult buyorder()
        {
            return View();
        }
        public IActionResult buyhold()
        {
            return View();
        }
        public IActionResult salereport()
        {
            return View();
        }
        public IActionResult registerapplication()
        {
            return View();
        }
        public IActionResult reportcatalog()
        {
            return View();
        }
        public IActionResult historyofpromotion()
        {
            return View();
        }
        public IActionResult travelpoints()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPointHistoryData(string reportType = "daily")
        {
            try
            {
                var endpoint = string.Equals(reportType, "deduction", StringComparison.OrdinalIgnoreCase)
                    ? "/Member/reportdailycutpoint"
                    : "/Member/reportdailypoint";

                var result = await _apiService.GetAsync<dynamic>(endpoint);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting point history data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลประวัติคะแนนได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSaleReportData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/reportsaleandexpainorder");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sale report data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลรายงานการขายได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPromotionHistoryData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/reportbonusbydate");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotion history data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลประวัติการส่งเสริมการขายได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTravelPointsData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/reportbonusbypaymentperiod");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting travel points data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลคะแนนท่องเที่ยวได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderHistoryData(string orderType = "personal")
        {
            try
            {
                var endpoint = orderType?.ToLowerInvariant() switch
                {
                    "hold" => "/Member/reportbuyholdorder",
                    "pending" => "/Member/reportpoorder",
                    _ => "/Member/reportbuytopuporder"
                };

                var result = await _apiService.GetAsync<dynamic>(endpoint);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order history data. orderType={OrderType}", orderType);
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลประวัติการสั่งซื้อได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRegisterApplicationData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/reportpositionhistory");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting register application data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลใบสมัครได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBonusReport(string reportType = "daily")
        {
            try
            {
                var endpoint = reportType switch
                {
                    "monthly" => "/Member/reportbonusbymonth",
                    "period"  => "/Member/reportbonusbypaymentperiod",
                    _         => "/Member/reportbonusbydate"
                };
                var result = await _apiService.GetAsync<dynamic>(endpoint);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bonus report data. reportType={ReportType}", reportType);
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลรายงานโบนัสได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBonusDetail(string detailType = "sponser")
        {
            try
            {
                var endpoint = detailType switch
                {
                    "rebate"   => "/Member/reportbonusbydatedetailrebate",
                    "binary"   => "/Member/reportbonusbydatedetailbinary",
                    "matching" => "/Member/reportbonusbydatedetailmatching",
                    "mobile"   => "/Member/reportbonusbydatedetailmobile",
                    _          => "/Member/reportbonusbydatedetailsponser"
                };
                var result = await _apiService.GetAsync<dynamic>(endpoint);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bonus detail data. detailType={DetailType}", detailType);
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลรายละเอียดโบนัสได้" });
            }
        }
    }
}

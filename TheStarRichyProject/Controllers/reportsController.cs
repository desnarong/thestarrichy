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
        public IActionResult poorder()
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
        public async Task<IActionResult> GetBonusDetail(string detailType = "sponser", string fromDate = "", string toDate = "")
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
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"{endpoint}{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bonus detail data. detailType={DetailType}", detailType);
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลรายละเอียดโบนัสได้" });
            }
        }

        // ─── View actions ──────────────────────────────────────────────────────────
        public IActionResult loginlog()       => View();
        public IActionResult cutpoint()       => View();
        public IActionResult dailypoint()     => View();

        // ─── Data endpoints ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetSaleReportDataV2(string fromDate = "", string toDate = "")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportsaleandexpainorder{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sale report");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBuyOrderData(string fromDate = "", string toDate = "")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportbuytopuporder{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting buy order data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBuyHoldData(string fromDate = "", string toDate = "", string checktype = "all")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                if (!string.IsNullOrWhiteSpace(checktype) && checktype != "all")
                    qs += (qs.Length > 0 ? "&" : "?") + $"checktype={Uri.EscapeDataString(checktype)}";
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportbuyholdorder{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting buy hold data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPoOrderData(string fromDate = "", string toDate = "")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportpoorder{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PO order data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLoginLogData(string fromDate = "", string toDate = "")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportlog{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login log data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPositionHistoryData(string fromDate = "", string toDate = "")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportpositionhistory{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting position history data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCutPointData(string fromDate = "", string toDate = "")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportdailycutpoint{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cut point data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyPointData(string fromDate = "", string toDate = "")
        {
            try
            {
                var qs = BuildDateQuery(fromDate, toDate);
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportdailypoint{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily point data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyPointSourceLeft(string balanceDate = "")
        {
            try
            {
                var qs = string.IsNullOrWhiteSpace(balanceDate) ? "" : $"?balancedate={Uri.EscapeDataString(balanceDate)}";
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportleftsourceofpv{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting left PV source");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyPointSourceRight(string balanceDate = "")
        {
            try
            {
                var qs = string.IsNullOrWhiteSpace(balanceDate) ? "" : $"?balancedate={Uri.EscapeDataString(balanceDate)}";
                var result = await _apiService.GetAsync<dynamic>($"/Member/reportrightsourceofpv{qs}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting right PV source");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryBuyerData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/teamnewbuy");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history buyer data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryRegisterData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/teamnewregister");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history register data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryQualifyData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/teamtotalpositionranking");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history qualify data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryBestSellerData()
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>("/Member/teambuyproduct");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history best seller data");
                return StatusCode(500, new { success = false, message = "ไม่สามารถดึงข้อมูลได้" });
            }
        }

        private static string BuildDateQuery(string fromDate, string toDate)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(fromDate)) parts.Add($"fromdate={Uri.EscapeDataString(fromDate)}");
            if (!string.IsNullOrWhiteSpace(toDate))   parts.Add($"todate={Uri.EscapeDataString(toDate)}");
            return parts.Count > 0 ? "?" + string.Join("&", parts) : "";
        }
    }
}

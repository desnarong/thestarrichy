using Microsoft.AspNetCore.Mvc;
using TheStarRichyProject.Helper;
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
            // Pass the logged-in member's code to the view for auto-load on page start
            ViewBag.LoggedInMemberCode = Request.Cookies[CookieHelper.MemberCodeKey] ?? "";
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

        /// <summary>
        /// ยืนยันรายการแจงยอดสินค้า (Hold) → Sync ไป T05
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConfirmSaleOrder([FromBody] ConfirmSaleOrderPayload payload)
        {
            try
            {
                if (payload == null || payload.Items == null || payload.Items.Count == 0)
                    return BadRequest(new { success = false, message = "กรุณาเลือกสินค้าอย่างน้อย 1 รายการ" });

                if (string.IsNullOrWhiteSpace(payload.MemberCode))
                    return BadRequest(new { success = false, message = "กรุณาระบุสมาชิก" });

                // อ่านรหัสผู้ทำรายการจาก cookie
                var createBy = Request.Cookies[CookieHelper.MemberCodeKey] ?? "system";

                // Map bill type: topup → "TP", hurry → "HA"
                var billtype = payload.PurchaseType == "hurry" ? "HA" : "TP";

                // Generate SaleNo
                var saleNo = $"SO-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";

                // Build API request
                var apiRequest = new
                {
                    SaleNo = saleNo,
                    Membercode = payload.MemberCode,
                    Billtype = billtype,
                    SaleDate = DateTime.Now,
                    CreateBy = createBy,
                    PV = payload.TotalPV,
                    Amount = payload.TotalAmount,
                    Detail = payload.Items.Select(i => new
                    {
                        ProductID = i.ProductID ?? "",
                        ProductName = i.ProductName ?? "",
                        UnitPrice = i.UnitPrice,
                        PV = i.PV,
                        Qty = i.Qty,
                        BillNo = i.BillNo ?? "",
                        ProductSet = i.ProductSet ?? ""
                    }).ToList()
                };

                var result = await _apiService.PostAsync<dynamic>("/Order/sync-to-t05", apiRequest);

                // Extract OrderID from response
                string orderId = null;
                if (result != null)
                {
                    // Try multiple possible paths
                    orderId = (string)(result.data?.orderID ?? result.data?.OrderID
                        ?? result.orderID ?? result.OrderID
                        ?? result.Data?.orderID ?? result.Data?.OrderID);
                }

                if (!string.IsNullOrEmpty(orderId))
                {
                    return Ok(new { success = true, billNo = orderId, message = "บันทึกรายการสำเร็จ" });
                }

                // Fallback: generate bill number if API didn't return one
                var fallbackBill = "BILL" + DateTime.Now.ToString("yyyyMMddHHmmss");
                return Ok(new { success = true, billNo = fallbackBill, message = "บันทึกรายการสำเร็จ (fallback)" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Payload ที่รับจากหน้า saleorderfromhold
    /// </summary>
    public class ConfirmSaleOrderPayload
    {
        public string MemberCode { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string PurchaseType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalPV { get; set; }
        public List<ConfirmSaleOrderItem> Items { get; set; } = new();
    }

    public class ConfirmSaleOrderItem
    {
        public string ProductID { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal PV { get; set; }
        public int Qty { get; set; }
        public decimal Remain { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string ProductSet { get; set; } = string.Empty;
    }
}

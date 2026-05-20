using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    /// <summary>
    /// BranchStock Controller สำหรับตรวจสอบสต็อกสินค้าตามสาขา
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BranchStockController : ControllerBase
    {
        private readonly IBranchStockService _branchStockService;
        private readonly ILogger<BranchStockController> _logger;

        public BranchStockController(
            IBranchStockService branchStockService,
            ILogger<BranchStockController> logger)
        {
            _branchStockService = branchStockService;
            _logger = logger;
        }

        /// <summary>
        /// Function 1: ดึงข้อมูลสินค้าจาก Branchcode
        /// GET: /api/BranchStock/stock-by-branch/{branchCode}
        /// </summary>
        [HttpGet("stock-by-branch/{branchCode}")]
        public async Task<IActionResult> GetStockByBranch(string branchCode)
        {
            try
            {
                if (string.IsNullOrEmpty(branchCode))
                {
                    return BadRequest(new { Success = false, Message = "BranchCode is required" });
                }

                var result = await _branchStockService.GetStockByBranchAsync(branchCode);

                return Ok(new
                {
                    Success = true,
                    Message = "ดึงข้อมูลสำเร็จ",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock by branch");
                return StatusCode(500, new { Success = false, Message = "เกิดข้อผิดพลาด" });
            }
        }

        /// <summary>
        /// Function 2: เช็คว่าสินค้าทั้งหมดใน array มีใน Branchcode นี้หรือไม่
        /// POST: /api/BranchStock/check-stock
        /// Body: { "branchCode": "xxx", "productCodes": ["code1", "code2", ...] }
        /// </summary>
        [HttpPost("check-stock")]
        public async Task<IActionResult> CheckStockByBranch([FromBody] CheckStockRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.BranchCode))
                {
                    return BadRequest(new { Success = false, Message = "BranchCode is required" });
                }

                if (request.ProductCodes == null || request.ProductCodes.Count == 0)
                {
                    return BadRequest(new { Success = false, Message = "ProductCodes array is required" });
                }

                var result = await _branchStockService.CheckStockByBranchAsync(request.BranchCode, request.ProductCodes);

                return Ok(new
                {
                    Success = true,
                    Message = result ? "พบสินค้าทั้งหมดในสาขา" : "ไม่พบสินค้าบางรายการในสาขา",
                    Data = new
                    {
                        IsAllFound = result,
                        BranchCode = request.BranchCode,
                        ProductCodes = request.ProductCodes
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock by branch");
                return StatusCode(500, new { Success = false, Message = "เกิดข้อผิดพลาด" });
            }
        }
    }

    /// <summary>
    /// Request model สำหรับ CheckStock
    /// </summary>
    public class CheckStockRequest
    {
        public string BranchCode { get; set; }
        public List<string> ProductCodes { get; set; }
    }
}

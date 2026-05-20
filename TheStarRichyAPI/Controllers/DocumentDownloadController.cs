using Microsoft.AspNetCore.Mvc;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    /// <summary>
    /// DocumentDownload Controller สำหรับดึงข้อมูลเอกสารดาวน์โหลดจาก view [000_download]
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class DocumentDownloadController : ControllerBase
    {
        private readonly IDocumentDownloadService _documentDownloadService;
        private readonly ILogger<DocumentDownloadController> _logger;

        public DocumentDownloadController(
            IDocumentDownloadService documentDownloadService,
            ILogger<DocumentDownloadController> logger)
        {
            _documentDownloadService = documentDownloadService;
            _logger = logger;
        }

        /// <summary>
        /// ดึงรายการเอกสารทั้งหมด
        /// GET: /api/DocumentDownload/all
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var result = await _documentDownloadService.GetAllDocumentsAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "ดึงข้อมูลสำเร็จ",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents");
                return StatusCode(500, new { Success = false, Message = "เกิดข้อผิดพลาดในการดึงข้อมูลเอกสาร" });
            }
        }
    }
}

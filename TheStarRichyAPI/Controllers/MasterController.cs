using Microsoft.AspNetCore.Mvc;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly ICheckwebStatusService  _checkwebStatusService;
        public MasterController(ICheckwebStatusService checkwebStatusService)
        {
            _checkwebStatusService = checkwebStatusService;
        }

        [HttpGet("webstatus")]
        public async Task<IActionResult> GetwebStatus()
        {
            try
            {
                var result = await _checkwebStatusService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

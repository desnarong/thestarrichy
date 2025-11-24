using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StaticController : ControllerBase
    {
        private readonly IStaticService _staticService;
        public StaticController(IStaticService staticService)
        {
            _staticService = staticService;
        }
        [HttpGet("banks")]
        public async Task<IActionResult> GetBanks()
        {
            try
            {
                var result = await _staticService.GetBankAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var result = await _staticService.GetCountryAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("countrybusinesses")]
        public async Task<IActionResult> GetCountryBusinesses()
        {
            try
            {
                var result = await _staticService.GetCountryBusinessAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts()
        {
            try
            {
                var result = await _staticService.GetDistrictAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("titlenames")]
        public async Task<IActionResult> GetTitlenames()
        {
            try
            {
                var result = await _staticService.GetTitlenameAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}

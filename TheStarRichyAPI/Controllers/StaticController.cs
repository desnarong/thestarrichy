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
        private readonly IBankService _bankService;
        public StaticController(IStaticService staticService, IBankService bankService)
        {
            _staticService = staticService;
            _bankService = bankService;
        }
        [HttpGet("paymentbank")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaymentBanks()
        {
            try
            {
                var result = await _bankService.GetDisplayAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("banks")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [HttpGet("addressmaster")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAddressMaster()
        {
            try
            {
                var result = await _staticService.GetAddressMasterAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("system")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemAsync()
        {
            try
            {
                var result = await _staticService.GetSystemAsync();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        //GetSystemAsync
    }
}

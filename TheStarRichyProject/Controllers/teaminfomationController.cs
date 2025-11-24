using Microsoft.AspNetCore.Mvc;
using TheStarRichyProject.Helper;
using TheStarRichyProject.Services;

namespace TheStarRichyProject.Controllers
{
    public class teaminfomationController : BaseController
    {
        private readonly IApiService _apiService;
        private readonly ILogger<teaminfomationController> _logger;

        public teaminfomationController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration config,
            IApiService apiService)
            : base(httpContextAccessor, loggerFactory, config)
        {
            _apiService = apiService;
            _logger = loggerFactory.CreateLogger<teaminfomationController>();
        }

        #region Binary Team

        public IActionResult teambinary()
        {
            var cookieCheck = CheckCookie();
            if (cookieCheck != null) return cookieCheck;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBinaryTeam(string memberCode)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>(
                    $"/Member/memberbinaryteam?membercode={memberCode}"
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting binary team");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindLeftBinary(string memberCode)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>(
                    $"/Member/findleftbinary?membercode={memberCode}"
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding left binary");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindRightBinary(string memberCode)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>(
                    $"/Member/findrightbinary?membercode={memberCode}"
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding right binary");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Sponsor Team

        public IActionResult sponsorteam()
        {
            var cookieCheck = CheckCookie();
            if (cookieCheck != null) return cookieCheck;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSponsorTeam(string memberCode)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>(
                    $"/Member/reportmembersponserteam?membercode={memberCode}"
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsor team");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Left Team

        public IActionResult leftteam()
        {
            var cookieCheck = CheckCookie();
            if (cookieCheck != null) return cookieCheck;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetLeftTeam(string memberCode)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>(
                    $"/Member/reportmemberleftteam?membercode={memberCode}"
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting left team");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLeftTeamSummary(string memberCode)
        {
            try
            {
                var packageResult = await _apiService.GetAsync<dynamic>(
                    $"/Member/reportmemberleftsumpackage?membercode={memberCode}"
                );

                var rankingResult = await _apiService.GetAsync<dynamic>(
                    $"/Member/reportmemberleftsumranking?membercode={memberCode}"
                );

                return Ok(new { packages = packageResult, rankings = rankingResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting left team summary");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Right Team

        public IActionResult rightteam()
        {
            var cookieCheck = CheckCookie();
            if (cookieCheck != null) return cookieCheck;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRightTeam(string memberCode)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>(
                    $"/Member/reportmemberrightteam?membercode={memberCode}"
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting right team");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRightTeamSummary(string memberCode)
        {
            try
            {
                var packageResult = await _apiService.GetAsync<dynamic>(
                    $"/Member/reportmemberrightsumpackage?membercode={memberCode}"
                );

                var rankingResult = await _apiService.GetAsync<dynamic>(
                    $"/Member/reportmemberrightsumranking?membercode={memberCode}"
                );

                return Ok(new { packages = packageResult, rankings = rankingResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting right team summary");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion
    }
}

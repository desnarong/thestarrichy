using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TheStarRichyApi.Services;

namespace TheStarRichyApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet("hello")]
        public IActionResult HelloWorld()
        {
            return Ok(new { message = "Hello World" });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> GetMemberSignIn([FromBody] SignInRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request parameters" });
            }

            try
            {
                var (result, token) = await _loginService.GetMemberSignInAsync(request.Username, request.Password, request.Passkey, request.IpAddress);
                if (result[0].MemberFlag == "Y")
                {
                    return Ok(new { MemberInfo = result, Token = token });
                }
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    public class SignInRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Password { get; set; }

        [Required]
        public string Passkey { get; set; }

        [Required]
        public string IpAddress { get; set; }
    }
}

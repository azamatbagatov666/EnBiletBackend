using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnBiletBackend.Attributes;
using EnBiletBackend.Models.Authentication;
using EnBiletBackend.Services;

namespace EnBiletBackend.Controllers
{
    [ApiController]
    [Route("Authentication")]
    public class AuthenticationController : Controller
    {
        public AuthenticationService _authenticationService;
        private readonly IConfiguration _config;
        public AuthenticationController(
            AuthenticationService authenticationService,
            IConfiguration config
        )
        {
            _authenticationService = authenticationService;
            _config = config;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] AuthenticateRequest request)
        {
            try
            {
                var result = _authenticationService.Authenticate(request);
                return Ok(result);
            }
            catch
            {
                // wrong credentials
                return Unauthorized(new { message = "Hatalı kullanıcı adı veya parola." });

            }
        }

        [HttpGet("TestLogin")]  
        [TheAuthorize]
        public bool TestLogin()
        {
            return HttpContext.Items["User"] != null;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public IActionResult Register(
            [FromBody] RegisterRequest request,
            [FromHeader(Name = "X-Admin-Secret")] string secret
        )
        {
            if (secret != _config["Admin:RegisterSecret"])
                return Unauthorized();

            _authenticationService.Register(request);
            return Ok();
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var result = _authenticationService.Refresh(request.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("Logout")]
        public IActionResult Logout([FromBody] RefreshRequest request)
        {
            _authenticationService.Logout(request.RefreshToken);
            return Ok();
        }



    }
}

using lordran_archives.DTOs;
using lordran_archives.Services;
using Microsoft.AspNetCore.Mvc;

namespace lordran_archives.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.Register(dto);
            if (result == null)
                return BadRequest(new { message = "Username already exists" });
            return Ok(new { message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.Login(dto);
            if (result == null)
                return BadRequest(new { message = "Invalid credentials" });
            return Ok(result);
        }
    }
}

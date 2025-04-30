using Microsoft.AspNetCore.Mvc;
using backen_it_support_utbildning.Services;

namespace backen_it_support_utbildning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService = new();

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.Login(request.Email, request.Password);
            if (result == null)
                return Unauthorized(new { message = "Fel email eller lösenord" });

            return Ok(result);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

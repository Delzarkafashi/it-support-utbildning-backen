using backen_it_support_utbildning.Services;
using Microsoft.AspNetCore.Mvc;

namespace backen_it_support_utbildning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AuthService _authService;

        public LoginController(AuthService authService)
        {
            _authService = authService;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.Login(request.Email, request.Password);

            if (token == null)
            {
                return Unauthorized(new { message = "Fel e-post eller lösenord" });
            }

            return Ok(new { token });
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backen_it_support_utbildning.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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

        [HttpGet("test-token")]
        public IActionResult GenerateTestToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("din_super_hemliga_nyckel_som_måste_va_minst_32_tecken"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("name", "Test User"),
                new Claim("email", "test@example.com"),
                new Claim("access_level", "3"),
                new Claim("category", "Matte"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "it_support_utbildning",
                audience: "it_support_utbildning",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = tokenString });
        }
    }
}

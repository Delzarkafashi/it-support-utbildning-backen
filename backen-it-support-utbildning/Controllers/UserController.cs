using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backen_it_support_utbildning.Services;
using backen_it_support_utbildning.Models;

namespace backen_it_support_utbildning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuthService _auth;

        public UserController()
        {
            _auth = new AuthService();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "Du är inloggad!",
                email,
                role
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var success = await _auth.RegisterUser(dto);
            if (!success)
                return BadRequest("E-post finns redan.");

            return Ok("Registrering lyckades.");
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _auth.Login(dto.Email, dto.Password);
                if (token == null)
                    return Unauthorized("Fel e-post eller lösenord.");

                return Ok(new { token });
            }
            catch (AccountLockedException ex)
            {
                return StatusCode(423, ex.Message);
            }
        }

    }
}

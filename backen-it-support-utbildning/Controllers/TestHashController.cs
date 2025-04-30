using Microsoft.AspNetCore.Mvc;

namespace backen_it_support_utbildning.Controllers
{
    [ApiController]
    [Route("api/testhash")]
    public class TestHashController : ControllerBase
    {
        [HttpPost]
        public IActionResult GenerateHash([FromBody] PasswordRequest request)
        {
            string hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);
            return Ok(new { HashedPassword = hashed });
        }
    }

    public class PasswordRequest
    {
        public string Password { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace backen_it_support_utbildning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Backend fungerar!" });
        }
    }
}

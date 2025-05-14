using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backen_it_support_utbildning.Models;
using backen_it_support_utbildning.Services;

namespace backen_it_support_utbildning.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _service;

        public QuizController(QuizService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || dto.Questions == null)
                return BadRequest("Ogiltig quiz-data");

            var quizId = await _service.CreateQuizAsync(dto);
            return Ok(new { id = quizId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes()
        {
            var user = HttpContext.User;

            var accessLevelClaim = user.FindFirst("access_level")?.Value;
            var category = user.FindFirst("category")?.Value;

            if (int.TryParse(accessLevelClaim, out int level))
            {
                if (level == 3 && !string.IsNullOrWhiteSpace(category))
                {
                    var filtered = await _service.GetQuizzesByCategoryAsync(category);
                    return Ok(filtered);
                }
            }

            var all = await _service.GetAllQuizzesAsync();
            return Ok(all);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] QuizCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Ogiltig quiz-data");

            try
            {
                await _service.UpdateQuizAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internt serverfel: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            try
            {
                await _service.DeleteQuizAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Kunde inte ta bort quiz: " + ex.Message);
            }
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _service.GetQuizzesByCategoryAsync(category);
            return Ok(result);
        }
    }
}

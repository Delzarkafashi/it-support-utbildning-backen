using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backen_it_support_utbildning.Services;
using backen_it_support_utbildning.Models;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace backen_it_support_utbildning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuthService _auth;
        private readonly string _connectionString;

        public UserController()
        {
            _auth = new AuthService();

            var config = new ConfigurationBuilder()
                .AddJsonFile("passwords.json")
                .Build();

            var dbPassword = config["DbPassword"]!;
            _connectionString = $"server=localhost;userid=root;password={dbPassword};database=it_support_utbildning;";
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
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var lockCheck = new MySqlCommand("SELECT lockout_until FROM login_attempts WHERE email = @Email", connection);
                lockCheck.Parameters.AddWithValue("@Email", dto.Email);
                var lockedUntilObj = await lockCheck.ExecuteScalarAsync();

                if (lockedUntilObj != null && lockedUntilObj != DBNull.Value)
                {
                    return Unauthorized(new { message = "⛔ Konto låst. Kontakta admin." });
                }

                var token = await _auth.Login(dto.Email, dto.Password);

                if (token == null)
                {
                    var updateCmd = new MySqlCommand(@"
                INSERT INTO login_attempts (email, attempts, last_attempt)
                VALUES (@Email, 1, NOW())
                ON DUPLICATE KEY UPDATE
                    attempts = attempts + 1,
                    last_attempt = NOW()", connection);

                    updateCmd.Parameters.AddWithValue("@Email", dto.Email);
                    await updateCmd.ExecuteNonQueryAsync();

                    var checkCmd = new MySqlCommand("SELECT attempts FROM login_attempts WHERE email = @Email", connection);
                    checkCmd.Parameters.AddWithValue("@Email", dto.Email);
                    var attempts = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (attempts >= 5)
                    {
                        var lockCmd = new MySqlCommand("UPDATE login_attempts SET lockout_until = NOW() WHERE email = @Email", connection);
                        lockCmd.Parameters.AddWithValue("@Email", dto.Email);
                        await lockCmd.ExecuteNonQueryAsync();

                        return Unauthorized(new { message = "⛔ Konto har låsts efter 5 misslyckade försök." });
                    }

                    return Unauthorized(new { message = "❌ Fel e-post eller lösenord." });
                }

                var clearCmd = new MySqlCommand("DELETE FROM login_attempts WHERE email = @Email", connection);
                clearCmd.Parameters.AddWithValue("@Email", dto.Email);
                await clearCmd.ExecuteNonQueryAsync();

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"❌ Serverfel: {ex.Message}" });
            }
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new MySqlCommand("SELECT id, name, email, category FROM users", connection);
                var reader = await cmd.ExecuteReaderAsync();

                var users = new List<object>();
                while (await reader.ReadAsync())
                {
                    users.Add(new
                    {
                        id = reader.GetInt32("id"),
                        name = reader.GetString("name"),
                        email = reader.GetString("email"),
                        category = reader["category"]?.ToString() ?? ""
                    });
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid hämtning av användare: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/category")]
        public async Task<IActionResult> UpdateUserCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new MySqlCommand("UPDATE users SET category = @category WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@category", dto.Category);
                cmd.Parameters.AddWithValue("@id", id);

                var result = await cmd.ExecuteNonQueryAsync();
                if (result > 0)
                    return NoContent();

                return NotFound("Användare hittades inte.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fel vid uppdatering: {ex.Message}");
            }
        }
    }
}

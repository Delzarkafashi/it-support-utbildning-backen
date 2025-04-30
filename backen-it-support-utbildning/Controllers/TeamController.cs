using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backen_it_support_utbildning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TeamController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetTeam()
        {
            var teamList = new List<object>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var command = new MySqlCommand(
                "SELECT name, role, email, phone, image_path, description FROM team_members",
                connection);

            try
            {
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    teamList.Add(new
                    {
                        Name = reader.GetString(0),
                        Role = reader.GetString(1),
                        Email = reader.GetString(2),
                        Phone = reader.GetString(3),
                        ImagePath = reader.GetString(4),
                        Description = reader.GetString(5)
                    });
                }

                return Ok(teamList);
            }
            catch (Exception ex)
            {
                return Problem($"Fel vid hämtning: {ex.Message}");
            }
        }
    }
}

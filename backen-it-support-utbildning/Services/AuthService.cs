using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace backen_it_support_utbildning.Services
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("passwords.json")
                .Build();

            var dbPassword = config["DbPassword"];
            _connectionString = $"server=localhost;userid=root;password={dbPassword};database=it_support_utbildning;";
        }

        public async Task<UserDto?> Login(string email, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var tables = new[] { "admins", "team_members", "users" };

            foreach (var table in tables)
            {
                var cmd = new MySqlCommand(
                    $"SELECT email, password_hash, access_level FROM {table} WHERE email = @email",
                    connection
                );
                cmd.Parameters.AddWithValue("@email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var storedHash = reader.GetString("password_hash");

                    if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                    {
                        return new UserDto
                        {
                            Email = reader.GetString("email"),
                            AccessLevel = reader.GetInt32("access_level")
                        };
                    }
                }

                await reader.DisposeAsync();
            }

            return null; 
        }
    }

    public class UserDto
    {
        public string Email { get; set; }
        public int AccessLevel { get; set; }
    }
}

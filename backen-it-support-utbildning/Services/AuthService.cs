using MySql.Data.MySqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using backen_it_support_utbildning.Models;
using System.Data;

namespace backen_it_support_utbildning.Services
{
    public class AuthService
    {
        private readonly string _connectionString;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;

        public AuthService()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("passwords.json")
                .Build();

            _jwtKey = config["JwtKey"]!;
            _jwtIssuer = config["JwtIssuer"]!;
            var dbPassword = config["DbPassword"]!;
            _connectionString = $"server=localhost;userid=root;password={dbPassword};database=it_support_utbildning;";
        }

        public async Task<bool> RegisterUser(RegisterDto dto)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @Email", connection);
            checkCmd.Parameters.AddWithValue("@Email", dto.Email);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
            if (exists) return false;

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var insertCmd = new MySqlCommand(
                "INSERT INTO users (name, email, password_hash, access_level, category) VALUES (@Name, @Email, @Hash, 3, @Category)",
                connection
            );
            insertCmd.Parameters.AddWithValue("@Name", dto.Name);
            insertCmd.Parameters.AddWithValue("@Email", dto.Email);
            insertCmd.Parameters.AddWithValue("@Hash", hash);
            insertCmd.Parameters.AddWithValue("@Category", dto.Category ?? "");
            await insertCmd.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<string?> Login(string email, string password)
        {
            var user = await LoginWithUserInfo(email, password);
            if (user == null)
                return null;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("name", user.Name),
                new Claim("email", user.Email),
                new Claim("access_level", user.AccessLevel.ToString()),
                new Claim("category", user.Category ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtIssuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDto?> LoginWithUserInfo(string email, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var tables = new[] { "admins", "team_members", "users" };

            foreach (var table in tables)
            {
                string query = table == "users"
                    ? $"SELECT name, email, password_hash, access_level, category FROM {table} WHERE email = @email"
                    : $"SELECT name, email, password_hash, access_level FROM {table} WHERE email = @email";

                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var storedHash = reader.GetString("password_hash");

                    if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                    {
                        var name = reader.GetString("name");
                        var emailResult = reader.GetString("email");
                        var accessLevel = reader.GetInt32("access_level");
                        var category = table == "users" ? reader["category"]?.ToString() ?? "" : "";

                        return new UserDto
                        {
                            Name = name,
                            Email = emailResult,
                            AccessLevel = accessLevel,
                            Category = category
                        };
                    }
                }
            }

            return null;
        }
    }

    public class UserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int AccessLevel { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}

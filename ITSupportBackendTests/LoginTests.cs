using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using backen_it_support_utbildning.Services;
using backen_it_support_utbildning.Models;

namespace backen_it_support_utbildning.Services
{
    public class LoginTests
    {
        private readonly IConfiguration _config;
        private readonly AuthService _service;

        public LoginTests()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("passwords.json")
                .Build();

            _service = new AuthService();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsAdmin()
        {
            var email = "delzar@gmail.com";
            var password = _config["AdminPassword"]!;

            var result = await _service.LoginWithUserInfo(email, password);

            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal(1, result.AccessLevel);
        }

        [Fact]
        public async Task Login_WithValidTeamMemberCredentials_ReturnsTeamMember()
        {
            var email = "rezgar@ITSupport&Utbildning.se";
            var password = _config["TeamPassword"]!;

            var result = await _service.LoginWithUserInfo(email, password);

            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal(2, result.AccessLevel);
        }

        [Fact]
        public async Task Login_WithValidUserCredentials_ReturnsUser()
        {
            var email = "Ali@ITSupport&Utbildning.se";
            var password = _config["UserPassword"]!;

            var result = await _service.LoginWithUserInfo(email, password);

            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal(3, result.AccessLevel);
        }

        [Fact]
        public async Task Register_NewUser_ReturnsTrue()
        {
            var uniqueEmail = $"testuser_{Guid.NewGuid()}@example.com";
            var dto = new RegisterDto
            {
                Name = "Test User",
                Email = uniqueEmail,
                Password = "test1234"
            };

            var result = await _service.RegisterUser(dto);

            Assert.True(result);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsFalse()
        {
            var email = "del@gmail.com";
            var dto = new RegisterDto
            {
                Name = "Redan Finns",
                Email = email,
                Password = "test1234"
            };

            var result = await _service.RegisterUser(dto);

            Assert.False(result);
        }
    }
}

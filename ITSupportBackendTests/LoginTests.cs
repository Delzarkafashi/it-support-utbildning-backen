using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using backen_it_support_utbildning.Services;

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
        public async Task Login_WithInvalidCredentials_ReturnsNull()
        {
            var result = await _service.LoginWithUserInfo("nobody@example.com", "wrongpassword");
            Assert.Null(result);
        }
    }
}

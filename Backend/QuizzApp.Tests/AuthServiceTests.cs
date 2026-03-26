using Microsoft.Extensions.Configuration;
using Moq;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;
using QuizzApp.Services;
using Xunit;

namespace QuizzApp.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepoMock;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IGenericRepository<User>>();

            // Minimal JWT config needed by AuthService
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "TestSecretKey_MustBeAtLeast32Characters!!" },
                { "JwtSettings:Issuer",    "TestIssuer" },
                { "JwtSettings:Audience",  "TestAudience" },
                { "JwtSettings:ExpiryInDays", "7" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(_userRepoMock.Object, _configuration);
        }

        // ── Register Tests ──────────────────────────────────────

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var dto = new RegisterDTO { FullName = "Test User", Email = "test@example.com", Password = "Pass123", Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var (success, message, data) = await _authService.RegisterAsync(dto);

            // Assert
            Assert.True(success);
            Assert.Equal("Registration successful.", message);
            Assert.NotNull(data);
            Assert.Equal("test@example.com", data.Email);
            Assert.Equal("QuizTaker", data.Role);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsFail()
        {
            // Arrange
            var dto = new RegisterDTO { FullName = "Test", Email = "test@example.com", Password = "Pass123", Role = "InvalidRole" };

            // Act
            var (success, message, data) = await _authService.RegisterAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Contains("Role must be", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsFail()
        {
            // Arrange
            var dto = new RegisterDTO { FullName = "Test", Email = "existing@example.com", Password = "Pass123", Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User> { new User { Email = "existing@example.com" } });

            // Act
            var (success, message, data) = await _authService.RegisterAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Email is already registered.", message);
            Assert.Null(data);
        }

        // ── Login Tests ─────────────────────────────────────────

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsSuccess()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Pass123");
            var user = new User { Id = 1, FullName = "Test User", Email = "test@example.com", PasswordHash = hashedPassword, Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User> { user });

            var dto = new LoginDTO { Email = "test@example.com", Password = "Pass123" };

            // Act
            var (success, message, data) = await _authService.LoginAsync(dto);

            // Assert
            Assert.True(success);
            Assert.Equal("Login successful.", message);
            Assert.NotNull(data);
            Assert.NotEmpty(data.Token);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsFail()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPass");
            var user = new User { Id = 1, Email = "test@example.com", PasswordHash = hashedPassword, Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User> { user });

            var dto = new LoginDTO { Email = "test@example.com", Password = "WrongPass" };

            // Act
            var (success, message, data) = await _authService.LoginAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Invalid email or password.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_ReturnsFail()
        {
            // Arrange
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User>());

            var dto = new LoginDTO { Email = "nobody@example.com", Password = "Pass123" };

            // Act
            var (success, message, data) = await _authService.LoginAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Invalid email or password.", message);
            Assert.Null(data);
        }

        // ── Reset Password Tests ────────────────────────────────

        [Fact]
        public async Task ResetPassword_WithValidEmail_ReturnsSuccess()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com", PasswordHash = "oldhash" };
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User> { user });
            _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var dto = new ResetPasswordDTO { Email = "test@example.com", NewPassword = "NewPass123" };

            // Act
            var (success, message) = await _authService.ResetPasswordAsync(dto);

            // Assert
            Assert.True(success);
            Assert.Equal("Password reset successfully.", message);
        }

        [Fact]
        public async Task ResetPassword_WithInvalidEmail_ReturnsFail()
        {
            // Arrange
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User>());

            var dto = new ResetPasswordDTO { Email = "nobody@example.com", NewPassword = "NewPass123" };

            // Act
            var (success, message) = await _authService.ResetPasswordAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("No account found with that email.", message);
        }
    }
}

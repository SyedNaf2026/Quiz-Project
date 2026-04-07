using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;
using QuizzApp.Services;
using Xunit;

namespace QuizzApp.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepoMock;
        private readonly IConfiguration _configuration;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IGenericRepository<User>>();

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
        }

        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private UserService CreateService(AppDbContext db) =>
            new UserService(_userRepoMock.Object, db, _configuration);

        // ── GetProfile Tests ────────────────────────────────────

        [Fact]
        public async Task GetProfile_UserNotFound_ReturnsNull()
        {
            using var db = CreateDb("US_GetProfile_Null");
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
            var service = CreateService(db);

            var result = await service.GetProfileAsync(userId: 99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfile_UserFound_ReturnsCorrectDTO()
        {
            using var db = CreateDb("US_GetProfile_Found");
            var user = new User { Id = 1, FullName = "Aravind", Email = "aravind@test.com", Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            var service = CreateService(db);

            var result = await service.GetProfileAsync(userId: 1);

            Assert.NotNull(result);
            Assert.Equal("Aravind", result.FullName);
            Assert.Equal("aravind@test.com", result.Email);
            Assert.Equal("QuizTaker", result.Role);
            Assert.Equal(1, result.Id);
        }

        // ── UpdateProfile Tests ─────────────────────────────────

        [Fact]
        public async Task UpdateProfile_UserNotFound_ReturnsFail()
        {
            using var db = CreateDb("US_Update_NotFound");
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
            var service = CreateService(db);

            var (success, message) = await service.UpdateProfileAsync(99, new UpdateProfileDTO { FullName = "Test", Email = "t@test.com" });

            Assert.False(success);
            Assert.Equal("User not found.", message);
        }

        [Fact]
        public async Task UpdateProfile_EmailTakenByAnotherUser_ReturnsFail()
        {
            using var db = CreateDb("US_Update_EmailTaken");
            var user = new User { Id = 1, FullName = "Aravind", Email = "aravind@test.com", Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            // Another user already has the new email
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User> { new User { Id = 2, Email = "taken@test.com" } });
            var service = CreateService(db);

            var dto = new UpdateProfileDTO { FullName = "Aravind", Email = "taken@test.com" };
            var (success, message) = await service.UpdateProfileAsync(1, dto);

            Assert.False(success);
            Assert.Equal("Email is already in use by another account.", message);
        }

        [Fact]
        public async Task UpdateProfile_SameEmail_DoesNotCheckDuplicate()
        {
            // Keeping the same email should not trigger duplicate check
            using var db = CreateDb("US_Update_SameEmail");
            var user = new User { Id = 1, FullName = "Aravind", Email = "aravind@test.com", Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new UpdateProfileDTO { FullName = "Aravind Updated", Email = "aravind@test.com" };
            var (success, message) = await service.UpdateProfileAsync(1, dto);

            Assert.True(success);
            Assert.Equal("Profile updated successfully.", message);
            // FindAsync should NOT be called since email didn't change
            _userRepoMock.Verify(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfile_ValidData_ReturnsSuccess()
        {
            using var db = CreateDb("US_Update_Valid");
            var user = new User { Id = 1, FullName = "Old Name", Email = "old@test.com", Role = "QuizTaker" };
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                         .ReturnsAsync(new List<User>());
            _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new UpdateProfileDTO { FullName = "New Name", Email = "new@test.com" };
            var (success, message) = await service.UpdateProfileAsync(1, dto);

            Assert.True(success);
            Assert.Equal("Profile updated successfully.", message);
        }

        // ── GetUserStats Tests ──────────────────────────────────

        [Fact]
        public async Task GetUserStats_NoResults_ReturnsEmptyStats()
        {
            // Arrange: user has no quiz results
            using var db = CreateDb("US_Stats_Empty");
            var service = CreateService(db);

            var result = await service.GetUserStatsAsync(userId: 1);

            Assert.Equal(0, result.TotalAttempts);
            Assert.Equal(0, result.AverageScore);
            Assert.Equal(0, result.BestScore);
            Assert.Equal(0, result.TotalQuizzesTaken);
        }

        [Fact]
        public async Task GetUserStats_WithResults_ReturnsCorrectTotals()
        {
            using var db = CreateDb("US_Stats_Totals");
            db.Categories.Add(new Category { Id = 1, Name = "Science" });
            db.Quizzes.Add(new Quiz { Id = 1, Title = "Quiz", CategoryId = 1, CreatedBy = 1 });
            db.QuizResults.AddRange(
                new QuizResult { UserId = 1, QuizId = 1, Score = 4, TotalQuestions = 5, Percentage = 80, CompletedAt = DateTime.UtcNow },
                new QuizResult { UserId = 1, QuizId = 1, Score = 2, TotalQuestions = 5, Percentage = 40, CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.GetUserStatsAsync(userId: 1);

            Assert.Equal(2, result.TotalAttempts);
            Assert.Equal(60.0, result.AverageScore);  // (80+40)/2
            Assert.Equal(80, result.BestScore);
        }

        [Fact]
        public async Task GetUserStats_CountsUniqueQuizzesTaken()
        {
            // 2 attempts on same quiz = 1 unique quiz taken
            using var db = CreateDb("US_Stats_Unique");
            db.Categories.Add(new Category { Id = 1, Name = "Science" });
            db.Quizzes.Add(new Quiz { Id = 1, Title = "Quiz", CategoryId = 1, CreatedBy = 1 });
            db.QuizResults.AddRange(
                new QuizResult { UserId = 1, QuizId = 1, Score = 3, TotalQuestions = 5, Percentage = 60, CompletedAt = DateTime.UtcNow },
                new QuizResult { UserId = 1, QuizId = 1, Score = 5, TotalQuestions = 5, Percentage = 100, CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.GetUserStatsAsync(userId: 1);

            Assert.Equal(2, result.TotalAttempts);
            Assert.Equal(1, result.TotalQuizzesTaken);  // same quiz twice = 1 unique
        }

        [Fact]
        public async Task GetUserStats_BestCategory_ReturnsHighestAverage()
        {
            using var db = CreateDb("US_Stats_BestCat");
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Science" },
                new Category { Id = 2, Name = "History" }
            );
            db.Quizzes.AddRange(
                new Quiz { Id = 1, Title = "Science Quiz", CategoryId = 1, CreatedBy = 1 },
                new Quiz { Id = 2, Title = "History Quiz", CategoryId = 2, CreatedBy = 1 }
            );
            db.QuizResults.AddRange(
                new QuizResult { UserId = 1, QuizId = 1, Score = 5, TotalQuestions = 5, Percentage = 100, CompletedAt = DateTime.UtcNow },
                new QuizResult { UserId = 1, QuizId = 2, Score = 2, TotalQuestions = 5, Percentage = 40,  CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.GetUserStatsAsync(userId: 1);

            // Science has 100%, History has 40% — Science should be best
            Assert.Equal("Science", result.BestCategory);
        }
    }
}

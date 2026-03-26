using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.Models;
using QuizzApp.Services;
using Xunit;

namespace QuizzApp.Tests
{
    public class LeaderboardServiceTests
    {
        // Creates a fresh in-memory database for each test
        private AppDbContext CreateDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        // ── GetLeaderboard Tests ────────────────────────────────

        [Fact]
        public async Task GetLeaderboard_ReturnsResults_OrderedByPercentage()
        {
            // Arrange: seed 2 results with different percentages
            using var db = CreateDb("Leaderboard_Order");
            var category = new Category { Id = 1, Name = "Science" };
            var quiz = new Quiz { Id = 1, Title = "Science Quiz", CategoryId = 1, CreatedBy = 1 };
            var user1 = new User { Id = 1, FullName = "Alice", Email = "alice@test.com", Role = "QuizTaker" };
            var user2 = new User { Id = 2, FullName = "Bob",   Email = "bob@test.com",   Role = "QuizTaker" };

            db.Categories.Add(category);
            db.Quizzes.Add(quiz);
            db.Users.AddRange(user1, user2);
            db.QuizResults.AddRange(
                new QuizResult { UserId = 1, QuizId = 1, Score = 3, TotalQuestions = 5, Percentage = 60, CompletedAt = DateTime.UtcNow },
                new QuizResult { UserId = 2, QuizId = 1, Score = 5, TotalQuestions = 5, Percentage = 100, CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var service = new LeaderboardService(db);

            // Act
            var result = (await service.GetLeaderboardAsync()).ToList();

            // Assert: highest percentage (Bob 100%) should be first
            Assert.Equal(2, result.Count);
            Assert.Equal("Bob", result[0].UserName);
            Assert.Equal("Alice", result[1].UserName);
        }

        [Fact]
        public async Task GetLeaderboard_AssignsRanksCorrectly()
        {
            // Arrange
            using var db = CreateDb("Leaderboard_Ranks");
            var category = new Category { Id = 1, Name = "Maths" };
            var quiz = new Quiz { Id = 1, Title = "Maths Quiz", CategoryId = 1, CreatedBy = 1 };
            var user = new User { Id = 1, FullName = "Alice", Email = "alice@test.com", Role = "QuizTaker" };

            db.Categories.Add(category);
            db.Quizzes.Add(quiz);
            db.Users.Add(user);
            db.QuizResults.Add(
                new QuizResult { UserId = 1, QuizId = 1, Score = 4, TotalQuestions = 5, Percentage = 80, CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var service = new LeaderboardService(db);

            // Act
            var result = (await service.GetLeaderboardAsync()).ToList();

            // Assert: first entry should have rank 1
            Assert.Equal(1, result[0].Rank);
        }

        [Fact]
        public async Task GetLeaderboard_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange: empty database
            using var db = CreateDb("Leaderboard_Empty");
            var service = new LeaderboardService(db);

            // Act
            var result = await service.GetLeaderboardAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLeaderboard_FilteredByCategory_ReturnsOnlyThatCategory()
        {
            // Arrange: 2 quizzes in different categories
            using var db = CreateDb("Leaderboard_Filter");
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Science" },
                new Category { Id = 2, Name = "History" }
            );
            db.Quizzes.AddRange(
                new Quiz { Id = 1, Title = "Science Quiz", CategoryId = 1, CreatedBy = 1 },
                new Quiz { Id = 2, Title = "History Quiz", CategoryId = 2, CreatedBy = 1 }
            );
            db.Users.Add(new User { Id = 1, FullName = "Alice", Email = "alice@test.com", Role = "QuizTaker" });
            db.QuizResults.AddRange(
                new QuizResult { UserId = 1, QuizId = 1, Score = 5, TotalQuestions = 5, Percentage = 100, CompletedAt = DateTime.UtcNow },
                new QuizResult { UserId = 1, QuizId = 2, Score = 3, TotalQuestions = 5, Percentage = 60,  CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var service = new LeaderboardService(db);

            // Act: filter by category 1 (Science only)
            var result = (await service.GetLeaderboardAsync(categoryId: 1)).ToList();

            // Assert: only 1 result for Science
            Assert.Single(result);
            Assert.Equal("Science Quiz", result[0].QuizTitle);
        }

        [Fact]
        public async Task GetLeaderboard_ReturnsCorrectScoreAndPercentage()
        {
            // Arrange
            using var db = CreateDb("Leaderboard_Score");
            db.Categories.Add(new Category { Id = 1, Name = "Tech" });
            db.Quizzes.Add(new Quiz { Id = 1, Title = "Tech Quiz", CategoryId = 1, CreatedBy = 1 });
            db.Users.Add(new User { Id = 1, FullName = "Alice", Email = "alice@test.com", Role = "QuizTaker" });
            db.QuizResults.Add(
                new QuizResult { UserId = 1, QuizId = 1, Score = 4, TotalQuestions = 5, Percentage = 80, CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var service = new LeaderboardService(db);

            // Act
            var result = (await service.GetLeaderboardAsync()).ToList();

            // Assert
            Assert.Equal(4, result[0].Score);
            Assert.Equal(5, result[0].TotalQuestions);
            Assert.Equal(80, result[0].Percentage);
        }
    }
}

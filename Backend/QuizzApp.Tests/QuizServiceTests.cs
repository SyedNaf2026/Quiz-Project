using Microsoft.EntityFrameworkCore;
using Moq;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;
using QuizzApp.Services;
using Xunit;

namespace QuizzApp.Tests
{
    public class QuizServiceTests
    {
        private readonly Mock<IGenericRepository<Quiz>> _quizRepoMock;
        private readonly Mock<IGenericRepository<Category>> _categoryRepoMock;
        private readonly Mock<INotificationService> _notifMock;

        public QuizServiceTests()
        {
            _quizRepoMock     = new Mock<IGenericRepository<Quiz>>();
            _categoryRepoMock = new Mock<IGenericRepository<Category>>();
            _notifMock        = new Mock<INotificationService>();

            // Notification calls are fire-and-forget in tests — just complete
            _notifMock.Setup(n => n.SendToAllTakersAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);
            _notifMock.Setup(n => n.SendToUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);
        }

        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private QuizService CreateService(AppDbContext db) =>
            new QuizService(_quizRepoMock.Object, _categoryRepoMock.Object, db, _notifMock.Object);

        // ── CreateQuiz Tests ────────────────────────────────────

        [Fact]
        public async Task CreateQuiz_CategoryNotFound_ReturnsFail()
        {
            using var db = CreateDb("QS_Create_NoCat");
            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);
            var service = CreateService(db);

            var dto = new CreateQuizDTO { Title = "Quiz", Description = "Desc", CategoryId = 99 };

            var (success, message, data) = await service.CreateQuizAsync(dto, creatorId: 1);

            Assert.False(success);
            Assert.Equal("Category not found.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task CreateQuiz_ValidData_ReturnsSuccess()
        {
            using var db = CreateDb("QS_Create_Valid");
            var category = new Category { Id = 1, Name = "Science" };
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
            _quizRepoMock.Setup(r => r.AddAsync(It.IsAny<Quiz>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new CreateQuizDTO { Title = "Science Quiz", Description = "Test", CategoryId = 1, Difficulty = "Easy" };

            var (success, message, data) = await service.CreateQuizAsync(dto, creatorId: 1);

            Assert.True(success);
            Assert.Equal("Quiz created successfully.", message);
            Assert.NotNull(data);
            Assert.Equal("Science Quiz", data.Title);
            Assert.Equal("Science", data.CategoryName);
            Assert.Equal("Easy", data.Difficulty);
        }

        // ── UpdateQuiz Tests ────────────────────────────────────

        [Fact]
        public async Task UpdateQuiz_QuizNotFound_ReturnsFail()
        {
            using var db = CreateDb("QS_Update_NotFound");
            _quizRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Quiz?)null);
            var service = CreateService(db);

            var (success, message) = await service.UpdateQuizAsync(99, new UpdateQuizDTO(), userId: 1);

            Assert.False(success);
            Assert.Equal("Quiz not found.", message);
        }

        [Fact]
        public async Task UpdateQuiz_WrongCreator_ReturnsFail()
        {
            using var db = CreateDb("QS_Update_WrongCreator");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            var service = CreateService(db);

            var (success, message) = await service.UpdateQuizAsync(1, new UpdateQuizDTO { CategoryId = 1 }, userId: 2);

            Assert.False(success);
            Assert.Equal("You can only update your own quizzes.", message);
        }

        [Fact]
        public async Task UpdateQuiz_ValidData_ReturnsSuccess()
        {
            using var db = CreateDb("QS_Update_Valid");
            var quiz = new Quiz { Id = 1, Title = "Old Title", CreatedBy = 1, CategoryId = 1 };
            var category = new Category { Id = 1, Name = "Science" };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
            _quizRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Quiz>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new UpdateQuizDTO { Title = "New Title", Description = "Desc", CategoryId = 1, IsActive = true };

            var (success, message) = await service.UpdateQuizAsync(1, dto, userId: 1);

            Assert.True(success);
            Assert.Equal("Quiz updated successfully.", message);
        }

        // ── DeleteQuiz Tests ────────────────────────────────────

        [Fact]
        public async Task DeleteQuiz_QuizNotFound_ReturnsFail()
        {
            using var db = CreateDb("QS_Delete_NotFound");
            _quizRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Quiz?)null);
            var service = CreateService(db);

            var (success, message) = await service.DeleteQuizAsync(99, userId: 1);

            Assert.False(success);
            Assert.Equal("Quiz not found.", message);
        }

        [Fact]
        public async Task DeleteQuiz_WrongCreator_ReturnsFail()
        {
            using var db = CreateDb("QS_Delete_WrongCreator");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            var service = CreateService(db);

            var (success, message) = await service.DeleteQuizAsync(1, userId: 2);

            Assert.False(success);
            Assert.Equal("You can only delete your own quizzes.", message);
        }

        [Fact]
        public async Task DeleteQuiz_ValidData_ReturnsSuccess()
        {
            using var db = CreateDb("QS_Delete_Valid");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            _quizRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Quiz>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var (success, message) = await service.DeleteQuizAsync(1, userId: 1);

            Assert.True(success);
            Assert.Equal("Quiz deleted successfully.", message);
        }

        // ── ToggleStatus Tests ──────────────────────────────────

        [Fact]
        public async Task ToggleStatus_ActiveQuiz_DeactivatesIt()
        {
            using var db = CreateDb("QS_Toggle_Deactivate");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1, IsActive = true };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            _quizRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Quiz>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var (success, message) = await service.ToggleQuizStatusAsync(1, userId: 1);

            Assert.True(success);
            Assert.Contains("deactivated", message);
        }

        [Fact]
        public async Task ToggleStatus_InactiveQuiz_ActivatesIt()
        {
            using var db = CreateDb("QS_Toggle_Activate");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1, IsActive = false };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            _quizRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Quiz>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var (success, message) = await service.ToggleQuizStatusAsync(1, userId: 1);

            Assert.True(success);
            Assert.Contains("activated", message);
        }

        // ── GetActiveQuizzes Tests ──────────────────────────────

        [Fact]
        public async Task GetActiveQuizzes_ReturnsOnlyActiveOnes()
        {
            using var db = CreateDb("QS_GetActive");
            db.Categories.Add(new Category { Id = 1, Name = "Science" });
            db.Users.Add(new User { Id = 1, FullName = "Creator", Email = "c@test.com", Role = "QuizCreator" });
            db.Quizzes.AddRange(
                new Quiz { Id = 1, Title = "Active Quiz",   CategoryId = 1, CreatedBy = 1, IsActive = true  },
                new Quiz { Id = 2, Title = "Inactive Quiz", CategoryId = 1, CreatedBy = 1, IsActive = false }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.GetActiveQuizzesAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Active Quiz", result[0].Title);
        }

        [Fact]
        public async Task GetActiveQuizzes_FilteredByCategory_ReturnsCorrect()
        {
            using var db = CreateDb("QS_GetActive_Filter");
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Science" },
                new Category { Id = 2, Name = "History" }
            );
            db.Users.Add(new User { Id = 1, FullName = "Creator", Email = "c@test.com", Role = "QuizCreator" });
            db.Quizzes.AddRange(
                new Quiz { Id = 1, Title = "Science Quiz", CategoryId = 1, CreatedBy = 1, IsActive = true },
                new Quiz { Id = 2, Title = "History Quiz", CategoryId = 2, CreatedBy = 1, IsActive = true }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.GetActiveQuizzesAsync(categoryId: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("Science Quiz", result[0].Title);
        }

        // ── GetQuizById Tests ───────────────────────────────────

        [Fact]
        public async Task GetQuizById_NotFound_ReturnsNull()
        {
            using var db = CreateDb("QS_GetById_Null");
            var service = CreateService(db);

            var result = await service.GetQuizByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetQuizById_Found_ReturnsCorrectDTO()
        {
            using var db = CreateDb("QS_GetById_Found");
            db.Categories.Add(new Category { Id = 1, Name = "Science" });
            db.Users.Add(new User { Id = 1, FullName = "Creator", Email = "c@test.com", Role = "QuizCreator" });
            db.Quizzes.Add(new Quiz { Id = 1, Title = "Science Quiz", CategoryId = 1, CreatedBy = 1, IsActive = true, Difficulty = "Hard" });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.GetQuizByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Science Quiz", result.Title);
            Assert.Equal("Science", result.CategoryName);
            Assert.Equal("Hard", result.Difficulty);
        }

        // ── GetQuizzesByCreator Tests ───────────────────────────

        [Fact]
        public async Task GetQuizzesByCreator_ReturnsOnlyThatCreatorsQuizzes()
        {
            using var db = CreateDb("QS_ByCreator");
            db.Categories.Add(new Category { Id = 1, Name = "Science" });
            db.Users.AddRange(
                new User { Id = 1, FullName = "Creator1", Email = "c1@test.com", Role = "QuizCreator" },
                new User { Id = 2, FullName = "Creator2", Email = "c2@test.com", Role = "QuizCreator" }
            );
            db.Quizzes.AddRange(
                new Quiz { Id = 1, Title = "Quiz by C1", CategoryId = 1, CreatedBy = 1, IsActive = true },
                new Quiz { Id = 2, Title = "Quiz by C2", CategoryId = 1, CreatedBy = 2, IsActive = true }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.GetQuizzesByCreatorAsync(creatorId: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("Quiz by C1", result[0].Title);
        }
    }
}

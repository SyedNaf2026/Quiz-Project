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
    public class QuizAttemptServiceTests
    {
        private readonly Mock<IGenericRepository<QuizResult>> _resultRepoMock;
        private readonly Mock<IGenericRepository<UserAnswer>> _answerRepoMock;
        private readonly Mock<IGenericRepository<Quiz>> _quizRepoMock;
        private readonly Mock<INotificationService> _notifMock;

        public QuizAttemptServiceTests()
        {
            _resultRepoMock = new Mock<IGenericRepository<QuizResult>>();
            _answerRepoMock = new Mock<IGenericRepository<UserAnswer>>();
            _quizRepoMock   = new Mock<IGenericRepository<Quiz>>();
            _notifMock      = new Mock<INotificationService>();

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

        private QuizAttemptService CreateService(AppDbContext db) =>
            new QuizAttemptService(_quizRepoMock.Object, _resultRepoMock.Object, _answerRepoMock.Object, db, _notifMock.Object);

        // Helper: seeds a quiz with questions and options into the InMemory DB
        private async Task SeedQuiz(AppDbContext db, bool isActive = true)
        {
            db.Quizzes.Add(new Quiz
            {
                Id = 1, Title = "Science Quiz", CategoryId = 1,
                CreatedBy = 1, IsActive = isActive
            });
            db.Questions.Add(new Question
            {
                Id = 1, QuizId = 1, QuestionText = "What is H2O?", QuestionType = "MultipleChoice"
            });
            db.Options.AddRange(
                new Option { Id = 1, QuestionId = 1, OptionText = "Water",  IsCorrect = true  },
                new Option { Id = 2, QuestionId = 1, OptionText = "Oxygen", IsCorrect = false }
            );
            await db.SaveChangesAsync();
        }

        // ── SubmitQuiz Tests ────────────────────────────────────

        [Fact]
        public async Task SubmitQuiz_QuizNotFound_ReturnsFail()
        {
            // Arrange: empty DB — no quiz exists
            using var db = CreateDb("QA_NotFound");
            _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<QuizResult, bool>>>()))
                           .ReturnsAsync(new List<QuizResult>());
            var service = CreateService(db);

            var dto = new SubmitQuizDTO { QuizId = 99, Answers = new List<AnswerDTO>() };

            // Act
            var (success, message, data) = await service.SubmitQuizAsync(dto, userId: 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Quiz not found.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task SubmitQuiz_InactiveQuiz_ReturnsFail()
        {
            // Arrange: quiz exists but IsActive = false
            using var db = CreateDb("QA_Inactive");
            await SeedQuiz(db, isActive: false);
            _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<QuizResult, bool>>>()))
                           .ReturnsAsync(new List<QuizResult>());
            var service = CreateService(db);

            var dto = new SubmitQuizDTO { QuizId = 1, Answers = new List<AnswerDTO>() };

            // Act
            var (success, message, _) = await service.SubmitQuizAsync(dto, userId: 1);

            // Assert
            Assert.False(success);
            Assert.Equal("This quiz is not active.", message);
        }

        [Fact]
        public async Task SubmitQuiz_CorrectAnswer_ScoreIsOne()
        {
            // Arrange: quiz with 1 question, user selects correct option (Id=1)
            using var db = CreateDb("QA_Correct");
            await SeedQuiz(db);
            _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<QuizResult, bool>>>()))
                           .ReturnsAsync(new List<QuizResult>());
            _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<QuizResult>())).Returns(Task.CompletedTask);
            _answerRepoMock.Setup(r => r.AddAsync(It.IsAny<UserAnswer>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new SubmitQuizDTO
            {
                QuizId = 1,
                Answers = new List<AnswerDTO>
                {
                    new AnswerDTO { QuestionId = 1, SelectedOptionId = 1, SelectedOptionIds = new List<int>() }
                }
            };

            // Act
            var (success, message, data) = await service.SubmitQuizAsync(dto, userId: 1);

            // Assert
            Assert.True(success);
            Assert.Equal(1, data!.Score);
            Assert.Equal(1, data.TotalQuestions);
            Assert.Equal(100, data.Percentage);
        }

        [Fact]
        public async Task SubmitQuiz_WrongAnswer_ScoreIsZero()
        {
            // Arrange: user selects wrong option (Id=2)
            using var db = CreateDb("QA_Wrong");
            await SeedQuiz(db);
            _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<QuizResult, bool>>>()))
                           .ReturnsAsync(new List<QuizResult>());
            _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<QuizResult>())).Returns(Task.CompletedTask);
            _answerRepoMock.Setup(r => r.AddAsync(It.IsAny<UserAnswer>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new SubmitQuizDTO
            {
                QuizId = 1,
                Answers = new List<AnswerDTO>
                {
                    new AnswerDTO { QuestionId = 1, SelectedOptionId = 2, SelectedOptionIds = new List<int>() }
                }
            };

            // Act
            var (success, _, data) = await service.SubmitQuizAsync(dto, userId: 1);

            // Assert
            Assert.True(success);
            Assert.Equal(0, data!.Score);
            Assert.Equal(0, data.Percentage);
        }

        [Fact]
        public async Task SubmitQuiz_ReturnsCorrectQuizTitle()
        {
            // Arrange
            using var db = CreateDb("QA_Title");
            await SeedQuiz(db);
            _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<QuizResult, bool>>>()))
                           .ReturnsAsync(new List<QuizResult>());
            _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<QuizResult>())).Returns(Task.CompletedTask);
            _answerRepoMock.Setup(r => r.AddAsync(It.IsAny<UserAnswer>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new SubmitQuizDTO { QuizId = 1, Answers = new List<AnswerDTO>() };

            // Act
            var (_, _, data) = await service.SubmitQuizAsync(dto, userId: 1);

            // Assert: quiz title should be returned in result
            Assert.Equal("Science Quiz", data!.QuizTitle);
        }

        [Fact]
        public async Task SubmitQuiz_AnswerBreakdown_IsCorrectFlag()
        {
            // Arrange: correct answer selected
            using var db = CreateDb("QA_Breakdown");
            await SeedQuiz(db);
            _resultRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<QuizResult, bool>>>()))
                           .ReturnsAsync(new List<QuizResult>());
            _resultRepoMock.Setup(r => r.AddAsync(It.IsAny<QuizResult>())).Returns(Task.CompletedTask);
            _answerRepoMock.Setup(r => r.AddAsync(It.IsAny<UserAnswer>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new SubmitQuizDTO
            {
                QuizId = 1,
                Answers = new List<AnswerDTO>
                {
                    new AnswerDTO { QuestionId = 1, SelectedOptionId = 1, SelectedOptionIds = new List<int>() }
                }
            };

            // Act
            var (_, _, data) = await service.SubmitQuizAsync(dto, userId: 1);

            // Assert: breakdown should mark the answer as correct
            Assert.Single(data!.AnswerBreakdown);
            Assert.True(data.AnswerBreakdown[0].IsCorrect);
        }

        // ── GetUserResults Tests ────────────────────────────────

        [Fact]
        public async Task GetUserResults_ReturnsResultsForCorrectUser()
        {
            // Arrange: 2 users, each with 1 result
            using var db = CreateDb("QA_GetResults");
            db.Quizzes.Add(new Quiz { Id = 1, Title = "Science Quiz", CategoryId = 1, CreatedBy = 1 });
            db.QuizResults.AddRange(
                new QuizResult { Id = 1, UserId = 1, QuizId = 1, Score = 4, TotalQuestions = 5, Percentage = 80, CompletedAt = DateTime.UtcNow },
                new QuizResult { Id = 2, UserId = 2, QuizId = 1, Score = 2, TotalQuestions = 5, Percentage = 40, CompletedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            // Act: get results for user 1 only
            var result = (await service.GetUserResultsAsync(userId: 1)).ToList();

            // Assert: only 1 result for user 1
            Assert.Single(result);
            Assert.Equal(4, result[0].Score);
        }

        [Fact]
        public async Task GetUserResults_WhenNoResults_ReturnsEmpty()
        {
            using var db = CreateDb("QA_GetResults_Empty");
            var service = CreateService(db);

            var result = await service.GetUserResultsAsync(userId: 99);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserResults_OrderedByMostRecentFirst()
        {
            // Arrange: 2 results for same user, different dates
            using var db = CreateDb("QA_GetResults_Order");
            db.Quizzes.Add(new Quiz { Id = 1, Title = "Quiz", CategoryId = 1, CreatedBy = 1 });
            var older = DateTime.UtcNow.AddDays(-2);
            var newer = DateTime.UtcNow;
            db.QuizResults.AddRange(
                new QuizResult { Id = 1, UserId = 1, QuizId = 1, Score = 2, TotalQuestions = 5, Percentage = 40, CompletedAt = older },
                new QuizResult { Id = 2, UserId = 1, QuizId = 1, Score = 5, TotalQuestions = 5, Percentage = 100, CompletedAt = newer }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            // Act
            var result = (await service.GetUserResultsAsync(userId: 1)).ToList();

            // Assert: most recent (score=5) should come first
            Assert.Equal(5, result[0].Score);
            Assert.Equal(2, result[1].Score);
        }
    }
}

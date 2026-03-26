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
    public class QuestionServiceTests
    {
        private readonly Mock<IGenericRepository<Question>> _questionRepoMock;
        private readonly Mock<IGenericRepository<Option>> _optionRepoMock;
        private readonly Mock<IGenericRepository<Quiz>> _quizRepoMock;

        public QuestionServiceTests()
        {
            _questionRepoMock = new Mock<IGenericRepository<Question>>();
            _optionRepoMock   = new Mock<IGenericRepository<Option>>();
            _quizRepoMock     = new Mock<IGenericRepository<Quiz>>();
        }

        // Creates a fresh in-memory DB for tests that need AppDbContext
        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private QuestionService CreateService(AppDbContext db) =>
            new QuestionService(_questionRepoMock.Object, _optionRepoMock.Object, _quizRepoMock.Object, db);

        // ── AddQuestion Tests ───────────────────────────────────

        [Fact]
        public async Task AddQuestion_QuizNotFound_ReturnsFail()
        {
            // Arrange: quiz repo returns null
            using var db = CreateDb("Q_QuizNotFound");
            _quizRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Quiz?)null);
            var service = CreateService(db);

            var dto = new CreateQuestionDTO { QuizId = 99, QuestionText = "Test?", QuestionType = "MultipleChoice", Options = new() };

            // Act
            var (success, message, data) = await service.AddQuestionAsync(dto, creatorId: 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Quiz not found.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task AddQuestion_WrongCreator_ReturnsFail()
        {
            // Arrange: quiz belongs to creator 1, but we pass creator 2
            using var db = CreateDb("Q_WrongCreator");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            var service = CreateService(db);

            var dto = new CreateQuestionDTO { QuizId = 1, QuestionText = "Test?", QuestionType = "MultipleChoice", Options = new() };

            // Act
            var (success, message, data) = await service.AddQuestionAsync(dto, creatorId: 2);

            // Assert
            Assert.False(success);
            Assert.Equal("You can only add questions to your own quizzes.", message);
        }

        [Fact]
        public async Task AddQuestion_LessThanTwoOptions_ReturnsFail()
        {
            // Arrange
            using var db = CreateDb("Q_LessThanTwo");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            var service = CreateService(db);

            var dto = new CreateQuestionDTO
            {
                QuizId = 1, QuestionText = "Test?", QuestionType = "MultipleChoice",
                Options = new List<CreateOptionDTO> { new CreateOptionDTO { OptionText = "A", IsCorrect = true } }
            };

            // Act
            var (success, message, _) = await service.AddQuestionAsync(dto, creatorId: 1);

            // Assert
            Assert.False(success);
            Assert.Equal("A question must have at least 2 options.", message);
        }

        [Fact]
        public async Task AddQuestion_NoCorrectOption_ReturnsFail()
        {
            // Arrange: 2 options but none marked correct
            using var db = CreateDb("Q_NoCorrect");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            var service = CreateService(db);

            var dto = new CreateQuestionDTO
            {
                QuizId = 1, QuestionText = "Test?", QuestionType = "MultipleChoice",
                Options = new List<CreateOptionDTO>
                {
                    new CreateOptionDTO { OptionText = "A", IsCorrect = false },
                    new CreateOptionDTO { OptionText = "B", IsCorrect = false }
                }
            };

            // Act
            var (success, message, _) = await service.AddQuestionAsync(dto, creatorId: 1);

            // Assert
            Assert.False(success);
            Assert.Equal("A question must have at least one correct option.", message);
        }

        [Fact]
        public async Task AddQuestion_MultipleChoice_WithTwoCorrect_ReturnsFail()
        {
            // Arrange: MultipleChoice must have exactly 1 correct
            using var db = CreateDb("Q_MC_TwoCorrect");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            var service = CreateService(db);

            var dto = new CreateQuestionDTO
            {
                QuizId = 1, QuestionText = "Test?", QuestionType = "MultipleChoice",
                Options = new List<CreateOptionDTO>
                {
                    new CreateOptionDTO { OptionText = "A", IsCorrect = true },
                    new CreateOptionDTO { OptionText = "B", IsCorrect = true }
                }
            };

            // Act
            var (success, message, _) = await service.AddQuestionAsync(dto, creatorId: 1);

            // Assert
            Assert.False(success);
            Assert.Equal("This question type must have exactly 1 correct option.", message);
        }

        [Fact]
        public async Task AddQuestion_ValidData_ReturnsSuccess()
        {
            // Arrange: valid MultipleChoice question
            using var db = CreateDb("Q_Valid");
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            _questionRepoMock.Setup(r => r.AddAsync(It.IsAny<Question>())).Returns(Task.CompletedTask);
            _optionRepoMock.Setup(r => r.AddAsync(It.IsAny<Option>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var dto = new CreateQuestionDTO
            {
                QuizId = 1, QuestionText = "What is 2+2?", QuestionType = "MultipleChoice",
                Options = new List<CreateOptionDTO>
                {
                    new CreateOptionDTO { OptionText = "3", IsCorrect = false },
                    new CreateOptionDTO { OptionText = "4", IsCorrect = true }
                }
            };

            // Act
            var (success, message, data) = await service.AddQuestionAsync(dto, creatorId: 1);

            // Assert
            Assert.True(success);
            Assert.Equal("Question added successfully.", message);
            Assert.NotNull(data);
            Assert.Equal("What is 2+2?", data.QuestionText);
            Assert.Equal(2, data.Options.Count);
        }

        // ── GetQuestionsByQuiz Tests ────────────────────────────

        [Fact]
        public async Task GetQuestionsByQuiz_ReturnsQuestionsWithOptions()
        {
            // Arrange: seed questions and options in InMemory DB
            using var db = CreateDb("Q_GetByQuiz");
            var question = new Question { Id = 1, QuizId = 1, QuestionText = "Capital of India?", QuestionType = "MultipleChoice" };
            db.Questions.Add(question);
            db.Options.AddRange(
                new Option { Id = 1, QuestionId = 1, OptionText = "Delhi",  IsCorrect = true  },
                new Option { Id = 2, QuestionId = 1, OptionText = "Mumbai", IsCorrect = false }
            );
            await db.SaveChangesAsync();

            var service = CreateService(db);

            // Act
            var result = (await service.GetQuestionsByQuizAsync(quizId: 1)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Capital of India?", result[0].QuestionText);
            Assert.Equal(2, result[0].Options.Count);
        }

        [Fact]
        public async Task GetQuestionsByQuiz_WhenNoQuestions_ReturnsEmpty()
        {
            using var db = CreateDb("Q_GetByQuiz_Empty");
            var service = CreateService(db);

            var result = await service.GetQuestionsByQuizAsync(quizId: 99);

            Assert.Empty(result);
        }

        // ── UpdateQuestion Tests ────────────────────────────────

        [Fact]
        public async Task UpdateQuestion_QuestionNotFound_ReturnsFail()
        {
            using var db = CreateDb("Q_Update_NotFound");
            _questionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Question?)null);
            var service = CreateService(db);

            var (success, message) = await service.UpdateQuestionAsync(99, "New text", creatorId: 1);

            Assert.False(success);
            Assert.Equal("Question not found.", message);
        }

        [Fact]
        public async Task UpdateQuestion_ValidData_ReturnsSuccess()
        {
            using var db = CreateDb("Q_Update_Valid");
            var question = new Question { Id = 1, QuizId = 1, QuestionText = "Old text", QuestionType = "MultipleChoice" };
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _questionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(question);
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            _questionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Question>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var (success, message) = await service.UpdateQuestionAsync(1, "New text", creatorId: 1);

            Assert.True(success);
            Assert.Equal("Question updated.", message);
        }

        // ── DeleteQuestion Tests ────────────────────────────────

        [Fact]
        public async Task DeleteQuestion_QuestionNotFound_ReturnsFail()
        {
            using var db = CreateDb("Q_Delete_NotFound");
            _questionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Question?)null);
            var service = CreateService(db);

            var (success, message) = await service.DeleteQuestionAsync(99, creatorId: 1);

            Assert.False(success);
            Assert.Equal("Question not found.", message);
        }

        [Fact]
        public async Task DeleteQuestion_ValidData_ReturnsSuccess()
        {
            using var db = CreateDb("Q_Delete_Valid");
            var question = new Question { Id = 1, QuizId = 1, QuestionText = "Test?", QuestionType = "MultipleChoice" };
            var quiz = new Quiz { Id = 1, Title = "Quiz", CreatedBy = 1, CategoryId = 1 };
            _questionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(question);
            _quizRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(quiz);
            _questionRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Question>())).Returns(Task.CompletedTask);
            var service = CreateService(db);

            var (success, message) = await service.DeleteQuestionAsync(1, creatorId: 1);

            Assert.True(success);
            Assert.Equal("Question deleted.", message);
        }
    }
}

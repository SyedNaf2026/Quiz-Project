using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    // QuizService handles all quiz CRUD operations for QuizCreators
    public class QuizService : IQuizService
    {
        private readonly IGenericRepository<Quiz> _quizRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public QuizService(
            IGenericRepository<Quiz> quizRepo,
            IGenericRepository<Category> categoryRepo,
            AppDbContext context,
            INotificationService notificationService)
        {
            _quizRepo = quizRepo;
            _categoryRepo = categoryRepo;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string Message, QuizDTO? Data)> CreateQuizAsync(CreateQuizDTO dto, int creatorId)
        {
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return (false, "Category not found.", null);

            var quiz = new Quiz
            {
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                TimeLimit = dto.TimeLimit,
                IsActive = true,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow,
                Difficulty = dto.Difficulty
            };

            await _quizRepo.AddAsync(quiz);

            // Notify all QuizTakers about the new quiz
            await _notificationService.SendToAllTakersAsync(
                $"New quiz available: \"{quiz.Title}\"", "quiz_added");

            var result = new QuizDTO
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CategoryName = category.Name,
                TimeLimit = quiz.TimeLimit,
                IsActive = quiz.IsActive,
                CreatorName = "",
                CreatedAt = quiz.CreatedAt,
                TotalQuestions = 0,
                Difficulty = quiz.Difficulty
            };

            return (true, "Quiz created successfully.", result);
        }

        // only the creator can update
        public async Task<(bool Success, string Message)> UpdateQuizAsync(int quizId, UpdateQuizDTO dto, int userId)
        {
            var quiz = await _quizRepo.GetByIdAsync(quizId);
            if (quiz == null) return (false, "Quiz not found.");
            if (quiz.CreatedBy != userId) return (false, "You can only update your own quizzes.");

            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null) return (false, "Category not found.");

            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.CategoryId = dto.CategoryId;
            quiz.TimeLimit = dto.TimeLimit;
            quiz.IsActive = dto.IsActive;
            quiz.Difficulty = dto.Difficulty;

            await _quizRepo.UpdateAsync(quiz);

            // Notify all QuizTakers about the quiz update
            await _notificationService.SendToAllTakersAsync(
                $"Quiz \"{quiz.Title}\" has been updated.", "quiz_updated");

            return (true, "Quiz updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteQuizAsync(int quizId, int userId)
        {
            var quiz = await _quizRepo.GetByIdAsync(quizId);
            if (quiz == null) return (false, "Quiz not found.");
            if (quiz.CreatedBy != userId) return (false, "You can only delete your own quizzes.");

            await _quizRepo.DeleteAsync(quiz);
            return (true, "Quiz deleted successfully.");
        }

        public async Task<(bool Success, string Message)> ToggleQuizStatusAsync(int quizId, int userId)
        {
            var quiz = await _quizRepo.GetByIdAsync(quizId);
            if (quiz == null) return (false, "Quiz not found.");
            if (quiz.CreatedBy != userId) return (false, "You can only manage your own quizzes.");

            quiz.IsActive = !quiz.IsActive;
            await _quizRepo.UpdateAsync(quiz);

            string status = quiz.IsActive ? "activated" : "deactivated";

            // Notify all QuizTakers when a quiz is deactivated
            if (!quiz.IsActive)
            {
                await _notificationService.SendToAllTakersAsync(
                    $"Quiz \"{quiz.Title}\" has been deactivated.", "quiz_deactivated");
            }

            return (true, $"Quiz {status} successfully.");
        }

        public async Task<IEnumerable<QuizDTO>> GetActiveQuizzesAsync(int? categoryId = null)
        {
            var query = _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Creator)
                .Include(q => q.Questions)
                .Where(q => q.IsActive);

            if (categoryId.HasValue)
                query = query.Where(q => q.CategoryId == categoryId.Value);

            var quizzes = await query.ToListAsync();

            return quizzes.Select(q => new QuizDTO
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                CategoryName = q.Category?.Name ?? "",
                TimeLimit = q.TimeLimit,
                IsActive = q.IsActive,
                CreatorName = q.Creator?.FullName ?? "",
                CreatedAt = q.CreatedAt,
                TotalQuestions = q.Questions.Count,
                Difficulty = q.Difficulty
            });
        }

        public async Task<IEnumerable<QuizDTO>> GetQuizzesByCreatorAsync(int creatorId)
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Creator)
                .Include(q => q.Questions)
                .Where(q => q.CreatedBy == creatorId)
                .ToListAsync();

            return quizzes.Select(q => new QuizDTO
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                CategoryName = q.Category?.Name ?? "",
                TimeLimit = q.TimeLimit,
                IsActive = q.IsActive,
                CreatorName = q.Creator?.FullName ?? "",
                CreatedAt = q.CreatedAt,
                TotalQuestions = q.Questions.Count,
                Difficulty = q.Difficulty
            });
        }

        public async Task<QuizDTO?> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Category)
                .Include(q => q.Creator)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return null;

            return new QuizDTO
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CategoryName = quiz.Category?.Name ?? "",
                TimeLimit = quiz.TimeLimit,
                IsActive = quiz.IsActive,
                CreatorName = quiz.Creator?.FullName ?? "",
                CreatedAt = quiz.CreatedAt,
                TotalQuestions = quiz.Questions.Count,
                Difficulty = quiz.Difficulty
            };
        }

        public async Task<object> GetQuizStatsAsync(int quizId, int creatorId)
        {
            var quiz = await _quizRepo.GetByIdAsync(quizId);
            if (quiz == null || quiz.CreatedBy != creatorId)
                return new { Error = "Quiz not found or access denied." };

            // Get all results for this quiz
            var results = await _context.QuizResults
                .Where(r => r.QuizId == quizId)
                .ToListAsync();

            int totalAttempts = results.Count;
            double averageScore = totalAttempts > 0 ? results.Average(r => r.Percentage) : 0;

            return new
            {
                QuizId = quizId,
                QuizTitle = quiz.Title,
                TotalAttempts = totalAttempts,
                AverageScore = Math.Round(averageScore, 2),
                HighestScore = totalAttempts > 0 ? results.Max(r => r.Percentage) : 0,
                LowestScore = totalAttempts > 0 ? results.Min(r => r.Percentage) : 0
            };
        }
    }
}

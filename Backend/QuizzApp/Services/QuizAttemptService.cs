using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly IGenericRepository<Quiz> _quizRepo;
        private readonly IGenericRepository<QuizResult> _resultRepo;
        private readonly IGenericRepository<UserAnswer> _answerRepo;
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public QuizAttemptService(
            IGenericRepository<Quiz> quizRepo,
            IGenericRepository<QuizResult> resultRepo,
            IGenericRepository<UserAnswer> answerRepo,
            AppDbContext context,
            INotificationService notificationService)
        {
            _quizRepo = quizRepo;
            _resultRepo = resultRepo;
            _answerRepo = answerRepo;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string Message, QuizResultDTO? Data)> SubmitQuizAsync(SubmitQuizDTO dto, int userId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

            if (quiz == null) return (false, "Quiz not found.", null);
            if (!quiz.IsActive) return (false, "This quiz is not active.", null);

            // Delete previous attempt so user can retake
            var existingAttempt = await _resultRepo.FindAsync(r => r.UserId == userId && r.QuizId == dto.QuizId);
            if (existingAttempt.Any())
            {
                foreach (var old in existingAttempt)
                    await _resultRepo.DeleteAsync(old);
                var oldAnswers = await _answerRepo.FindAsync(a => a.UserId == userId && a.QuizId == dto.QuizId);
                foreach (var old in oldAnswers)
                    await _answerRepo.DeleteAsync(old);
            }

            int score = 0;
            int totalQuestions = quiz.Questions.Count;
            var answerBreakdown = new List<AnswerResultDTO>();

            foreach (var answer in dto.Answers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question == null) continue;

                bool isCorrect = false;
                var breakdown = new AnswerResultDTO
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType
                };

                if (question.QuestionType == "MultipleAnswer")
                {
                    // Exact set match required for full point
                    var correctIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
                    var selectedIds = answer.SelectedOptionIds.ToHashSet();
                    isCorrect = correctIds.SetEquals(selectedIds);

                    breakdown.SelectedOptionIds = answer.SelectedOptionIds;
                    breakdown.SelectedOptionTexts = answer.SelectedOptionIds
                        .Select(id => question.Options.FirstOrDefault(o => o.Id == id)?.OptionText ?? "")
                        .ToList();
                    breakdown.CorrectOptionIds = correctIds.ToList();
                    breakdown.CorrectOptionTexts = question.Options
                        .Where(o => o.IsCorrect).Select(o => o.OptionText).ToList();

                    // Save one UserAnswer row per selected option
                    foreach (var optId in answer.SelectedOptionIds)
                    {
                        await _answerRepo.AddAsync(new UserAnswer
                        {
                            UserId = userId,
                            QuizId = dto.QuizId,
                            QuestionId = answer.QuestionId,
                            SelectedOptionId = optId
                        });
                    }
                }
                else
                {
                    // Single-answer types
                    var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                    var selectedOption = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId);
                    isCorrect = correctOption != null && answer.SelectedOptionId == correctOption.Id;

                    breakdown.SelectedOptionId = answer.SelectedOptionId;
                    breakdown.SelectedOptionText = selectedOption?.OptionText ?? "Not answered";
                    breakdown.CorrectOptionId = correctOption?.Id ?? 0;
                    breakdown.CorrectOptionText = correctOption?.OptionText ?? "Not found";

                    await _answerRepo.AddAsync(new UserAnswer
                    {
                        UserId = userId,
                        QuizId = dto.QuizId,
                        QuestionId = answer.QuestionId,
                        SelectedOptionId = answer.SelectedOptionId
                    });
                }

                if (isCorrect) score++;
                breakdown.IsCorrect = isCorrect;
                answerBreakdown.Add(breakdown);
            }

            double percentage = totalQuestions > 0
                ? Math.Round((double)score / totalQuestions * 100, 2)
                : 0;

            var quizResult = new QuizResult
            {
                UserId = userId,
                QuizId = dto.QuizId,
                Score = score,
                TotalQuestions = totalQuestions,
                Percentage = percentage,
                CompletedAt = DateTime.UtcNow
            };
            await _resultRepo.AddAsync(quizResult);

            // ── Leaderboard notification logic ──────────────────────────
            // Get the overall leaderboard BEFORE this result was saved to find previous #1
            // We compare by looking at all results excluding the one just saved
            var previousTop = await _context.QuizResults
                .Include(r => r.User)
                .Where(r => !(r.UserId == userId && r.QuizId == dto.QuizId))
                .OrderByDescending(r => r.Percentage)
                .ThenByDescending(r => r.Score)
                .FirstOrDefaultAsync();

            // Get new #1 after save
            var newTop = await _context.QuizResults
                .Include(r => r.User)
                .OrderByDescending(r => r.Percentage)
                .ThenByDescending(r => r.Score)
                .FirstOrDefaultAsync();

            bool leaderboardChanged = newTop != null &&
                (previousTop == null || newTop.UserId != previousTop.UserId);

            if (leaderboardChanged && newTop != null)
            {
                string newTopName = newTop.User?.FullName ?? "Someone";

                // Notify all QuizTakers about the leaderboard change
                await _notificationService.SendToAllTakersAsync(
                    $"🏆 Leaderboard updated! {newTopName} is now #1!", "leaderboard_update");

                // Notify the displaced #1 specifically
                if (previousTop != null && previousTop.UserId != newTop.UserId)
                {
                    await _notificationService.SendToUserAsync(
                        previousTop.UserId,
                        $"You've been overtaken! {newTopName} is now #1 on the leaderboard.",
                        "rank_lost");
                }
            }
            // ────────────────────────────────────────────────────────────

            return (true, "Quiz submitted successfully.", new QuizResultDTO
            {
                ResultId = quizResult.Id,
                QuizId = dto.QuizId,
                QuizTitle = quiz.Title,
                Score = score,
                TotalQuestions = totalQuestions,
                Percentage = percentage,
                CompletedAt = quizResult.CompletedAt,
                AnswerBreakdown = answerBreakdown
            });
        }

        public async Task<IEnumerable<QuizResultDTO>> GetUserResultsAsync(int userId)
        {
            var results = await _context.QuizResults
                .Include(r => r.Quiz)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            return results.Select(r => new QuizResultDTO
            {
                ResultId = r.Id,
                QuizId = r.QuizId,
                QuizTitle = r.Quiz?.Title ?? "",
                Score = r.Score,
                TotalQuestions = r.TotalQuestions,
                Percentage = r.Percentage,
                CompletedAt = r.CompletedAt,
                AnswerBreakdown = new List<AnswerResultDTO>()
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    // QuestionService handles creating, updating, and deleting questions and their options
    public class QuestionService : IQuestionService
    {
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<Option> _optionRepo;
        private readonly IGenericRepository<Quiz> _quizRepo;
        private readonly AppDbContext _context;

        public QuestionService(
            IGenericRepository<Question> questionRepo,
            IGenericRepository<Option> optionRepo,
            IGenericRepository<Quiz> quizRepo,
            AppDbContext context)
        {
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _quizRepo = quizRepo;
            _context = context;
        }

        public async Task<(bool Success, string Message, QuestionDTO? Data)> AddQuestionAsync(CreateQuestionDTO dto, int creatorId)
        {
            var quiz = await _quizRepo.GetByIdAsync(dto.QuizId);
            if (quiz == null) return (false, "Quiz not found.", null);
            if (quiz.CreatedBy != creatorId) return (false, "You can only add questions to your own quizzes.", null);

            if (dto.Options.Count < 2)
                return (false, "A question must have at least 2 options.", null);

            int correctCount = dto.Options.Count(o => o.IsCorrect);
            if (correctCount == 0)
                return (false, "A question must have at least one correct option.", null);

            // MultipleChoice, TrueFalse, YesNo must have exactly 1 correct
            if (dto.QuestionType != "MultipleAnswer" && correctCount != 1)
                return (false, "This question type must have exactly 1 correct option.", null);

            var question = new Question
            {
                QuizId = dto.QuizId,
                QuestionText = dto.QuestionText,
                QuestionType = dto.QuestionType
            };

            await _questionRepo.AddAsync(question);

            var optionDTOs = new List<OptionDTO>();
            foreach (var optDto in dto.Options)
            {
                var option = new Option
                {
                    QuestionId = question.Id,
                    OptionText = optDto.OptionText,
                    IsCorrect = optDto.IsCorrect
                };
                await _optionRepo.AddAsync(option);
                optionDTOs.Add(new OptionDTO { Id = option.Id, OptionText = option.OptionText, IsCorrect = option.IsCorrect });
            }

            var result = new QuestionDTO
            {
                Id = question.Id,
                QuizId = question.QuizId,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                Options = optionDTOs
            };

            return (true, "Question added successfully.", result);
        }

        public async Task<IEnumerable<QuestionDTO>> GetQuestionsByQuizAsync(int quizId)
        {
            var questions = await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            return questions.Select(q => new QuestionDTO
            {
                Id = q.Id,
                QuizId = q.QuizId,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options.Select(o => new OptionDTO
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            });
        }

        public async Task<(bool Success, string Message)> UpdateQuestionAsync(int questionId, string newText, int creatorId)
        {
            var question = await _questionRepo.GetByIdAsync(questionId);
            if (question == null) return (false, "Question not found.");

            // Make sure the quiz belongs to this creator
            var quiz = await _quizRepo.GetByIdAsync(question.QuizId);
            if (quiz == null || quiz.CreatedBy != creatorId)
                return (false, "Access denied.");

            question.QuestionText = newText;
            await _questionRepo.UpdateAsync(question);
            return (true, "Question updated.");
        }

        public async Task<(bool Success, string Message)> DeleteQuestionAsync(int questionId, int creatorId)
        {
            var question = await _questionRepo.GetByIdAsync(questionId);
            if (question == null) return (false, "Question not found.");

            var quiz = await _quizRepo.GetByIdAsync(question.QuizId);
            if (quiz == null || quiz.CreatedBy != creatorId)
                return (false, "Access denied.");

            await _questionRepo.DeleteAsync(question);
            return (true, "Question deleted.");
        }
    }
}

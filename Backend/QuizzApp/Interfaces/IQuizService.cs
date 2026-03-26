using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface IQuizService
    {
        Task<(bool Success, string Message, QuizDTO? Data)>CreateQuizAsync(CreateQuizDTO dto, int creatorId);

        Task<(bool Success, string Message)>UpdateQuizAsync(int quizId, UpdateQuizDTO dto, int userId);

        Task<(bool Success, string Message)>DeleteQuizAsync(int quizId, int userId);

        Task<(bool Success, string Message)>ToggleQuizStatusAsync(int quizId, int userId);

        Task<IEnumerable<QuizDTO>>GetActiveQuizzesAsync(int? categoryId = null);

        Task<IEnumerable<QuizDTO>>GetQuizzesByCreatorAsync(int creatorId);

        Task<QuizDTO?>GetQuizByIdAsync(int quizId);

        Task<object>GetQuizStatsAsync(int quizId, int creatorId);
    }
}
using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface IQuizAttemptService
    {
        Task<(bool Success, string Message, QuizResultDTO? Data)>SubmitQuizAsync(SubmitQuizDTO dto, int userId);

        Task<IEnumerable<QuizResultDTO>>GetUserResultsAsync(int userId);
    }
}
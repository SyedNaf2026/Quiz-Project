using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface IQuestionService
    {
        Task<(bool Success, string Message, QuestionDTO? Data)>AddQuestionAsync(CreateQuestionDTO dto, int creatorId);

        Task<IEnumerable<QuestionDTO>>GetQuestionsByQuizAsync(int quizId);

        Task<(bool Success, string Message)>UpdateQuestionAsync(int questionId, string newText, int creatorId);

        Task<(bool Success, string Message)>DeleteQuestionAsync(int questionId, int creatorId);
    }
}
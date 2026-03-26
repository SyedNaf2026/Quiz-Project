namespace QuizzApp.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public int QuestionId { get; set; }

        public int SelectedOptionId { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Question? Question { get; set; }
    }
}

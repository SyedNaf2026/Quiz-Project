namespace QuizzApp.Models
{
    // Stores the result after a user completes a quiz
    public class QuizResult
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public int Score { get; set; }

        public int TotalQuestions { get; set; }

        // Score as a percentage (Score / TotalQuestions * 100)
        public double Percentage { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Quiz? Quiz { get; set; }
    }
}

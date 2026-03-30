namespace QuizzApp.Models
{
    // Stores a group member's quiz submission with validation status
    public class GroupQuizResult
    {
        public int Id { get; set; }

        public int GroupQuizId { get; set; }

        public int UserId { get; set; }

        // Links to the standard QuizResult for score details
        public int QuizResultId { get; set; }

        // "Pending" | "Approved"
        public string ValidationStatus { get; set; } = "Pending";

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public GroupQuiz? GroupQuiz { get; set; }
        public User? User { get; set; }
        public QuizResult? QuizResult { get; set; }
    }
}

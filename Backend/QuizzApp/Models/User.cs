namespace QuizzApp.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "QuizTaker";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation: quizzes created by this user
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        // Navigation: results of quizzes taken by this user
        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();

        // Navigation: answers submitted by this user
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}

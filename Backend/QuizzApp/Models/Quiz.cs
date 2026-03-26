namespace QuizzApp.Models
{
    // Represents a quiz created by a QuizCreator
    public class Quiz
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Foreign key: which category this quiz belongs to
        public int CategoryId { get; set; }

        public int? TimeLimit { get; set; }

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Difficulty { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public User? Creator { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
    }
}

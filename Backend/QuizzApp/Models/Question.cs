namespace QuizzApp.Models
{
    public class Question
    {
        public int Id { get; set; }

        // Foreign key: which quiz this question belongs to
        public int QuizId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        // MultipleChoice | MultipleAnswer | TrueFalse | YesNo
        public string QuestionType { get; set; } = "MultipleChoice";

        // Navigation properties
        public Quiz? Quiz { get; set; }
        public ICollection<Option> Options { get; set; } = new List<Option>();
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}

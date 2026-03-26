namespace QuizzApp.Models
{
    public class Option
    {
        public int Id { get; set; }

        // Foreign key: which question this option belongs to
        public int QuestionId { get; set; }

        public string OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;

        // Navigation
        public Question? Question { get; set; }
    }
}

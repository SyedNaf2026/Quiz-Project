namespace QuizzApp.DTOs
{
    public class CreateOptionDTO
    {
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
    }

    public class CreateQuestionDTO
    {
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        // MultipleChoice | MultipleAnswer | TrueFalse | YesNo
        public string QuestionType { get; set; } = "MultipleChoice";
        public List<CreateOptionDTO> Options { get; set; } = new List<CreateOptionDTO>();
    }

    public class QuestionDTO
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MultipleChoice";
        public List<OptionDTO> Options { get; set; } = new List<OptionDTO>();
    }

    public class OptionDTO
    {
        public int Id { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}

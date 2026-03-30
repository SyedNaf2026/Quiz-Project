namespace QuizzApp.Models
{
    public class GroupQuiz
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int QuizId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // True only when the GroupManager created this quiz specifically for the group
        public bool RequiresValidation { get; set; } = false;

        public Group? Group { get; set; }
        public Quiz? Quiz { get; set; }
        public ICollection<GroupQuizResult> GroupQuizResults { get; set; } = new List<GroupQuizResult>();
    }
}

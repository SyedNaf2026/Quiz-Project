namespace QuizzApp.Models
{
    // Join table: which users (QuizTakers) belong to which group
    public class GroupMember
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Group? Group { get; set; }
        public User? User { get; set; }
    }
}

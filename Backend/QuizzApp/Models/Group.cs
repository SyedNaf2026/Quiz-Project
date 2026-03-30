namespace QuizzApp.Models
{
    public class Group
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // The GroupManager who created this group
        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? Creator { get; set; }
        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public ICollection<GroupQuiz> GroupQuizzes { get; set; } = new List<GroupQuiz>();
    }
}

namespace QuizzApp.Models
{
    // Stores a persistent in-app notification for a specific user
    public class Notification
    {
        public int Id { get; set; }

        // Who receives this notification (null = broadcast to all)
        public int? UserId { get; set; }

        public string Message { get; set; } = string.Empty;

        // "quiz_added" | "quiz_deactivated" | "leaderboard_update" | "rank_lost"
        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
    }
}

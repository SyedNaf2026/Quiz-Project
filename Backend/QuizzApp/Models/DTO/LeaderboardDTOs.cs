namespace QuizzApp.DTOs
{
    public class LeaderboardDTO
    {
        public int Rank { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    // Used to filter leaderboard by category (optional)
    public class LeaderboardFilterDTO
    {
        public int? CategoryId { get; set; }
    }
}

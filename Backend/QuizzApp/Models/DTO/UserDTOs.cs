namespace QuizzApp.DTOs
{
    // Used to update user profile info
    public class UpdateProfileDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // Returned when viewing a user profile
    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UserStatsDTO
    {
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public string BestCategory { get; set; } = string.Empty;
        public double BestScore { get; set; }
        public int TotalQuizzesTaken { get; set; }
    }

    public class UpgradeResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

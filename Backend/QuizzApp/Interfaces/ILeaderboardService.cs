using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface ILeaderboardService
    {
        // optionally filtered by category
        Task<IEnumerable<LeaderboardDTO>> GetLeaderboardAsync(int? categoryId = null);
    }
}
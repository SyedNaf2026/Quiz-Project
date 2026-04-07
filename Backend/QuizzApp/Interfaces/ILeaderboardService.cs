using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface ILeaderboardService
    {
        Task<IEnumerable<LeaderboardDTO>> GetLeaderboardAsync(int? categoryId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
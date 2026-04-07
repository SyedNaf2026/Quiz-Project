using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;

namespace QuizzApp.Services
{
    // LeaderboardService returns top scorers, optionally filtered by category
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _context;

        public LeaderboardService(AppDbContext context)
        {
            _context = context;
        }

        // Get leaderboard by quiz category
        public async Task<IEnumerable<LeaderboardDTO>> GetLeaderboardAsync(int? categoryId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.QuizResults
                .Include(r => r.User)
                .Include(r => r.Quiz)
                    .ThenInclude(q => q!.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(r => r.Quiz!.CategoryId == categoryId.Value);

            // Filter by date range
            if (fromDate.HasValue)
                query = query.Where(r => r.CompletedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.CompletedAt <= toDate.Value);

            var results = await query
                .OrderByDescending(r => r.Percentage)
                .ThenByDescending(r => r.Score)
                .Take(50)
                .ToListAsync();

            int rank = 1;
            return results.Select(r => new LeaderboardDTO
            {
                Rank = rank++,
                UserName = r.User?.FullName ?? "Unknown",
                QuizTitle = r.Quiz?.Title ?? "Unknown",
                Score = r.Score,
                TotalQuestions = r.TotalQuestions,
                Percentage = r.Percentage,
                CompletedAt = r.CompletedAt
            });
        }
    }
}

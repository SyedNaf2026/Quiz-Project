using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;

namespace QuizzApp.Controllers
{
    // LeaderboardController returns top scorers
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Any logged-in user can view the leaderboard
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        // GET api/leaderboard?categoryId=1&fromDate=2025-01-01&toDate=2025-12-31
        [HttpGet]
        public async Task<IActionResult> GetLeaderboard(
            [FromQuery] int? categoryId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            // If toDate provided, include the full end day
            var adjustedToDate = toDate.HasValue ? toDate.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null;
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(categoryId, fromDate, adjustedToDate);
            return Ok(ApiResponse<IEnumerable<LeaderboardDTO>>.Ok(leaderboard));
        }
    }
}

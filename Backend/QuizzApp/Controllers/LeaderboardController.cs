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

        // GET api/leaderboard?categoryId=1
        // Get top scorers (optionally filtered by quiz category)
        [HttpGet]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int? categoryId = null)
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(categoryId);
            return Ok(ApiResponse<IEnumerable<LeaderboardDTO>>.Ok(leaderboard));
        }
    }
}

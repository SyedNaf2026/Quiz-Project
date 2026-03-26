using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;

namespace QuizzApp.Controllers
{
    // QuizAttemptController lets QuizTakers take quizzes and see their results
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "QuizTaker")]
    public class QuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _attemptService;

        public QuizAttemptController(IQuizAttemptService attemptService)
        {
            _attemptService = attemptService;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // POST api/quizattempt/submit
        // Submit answers for a quiz - score is calculated automatically
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitQuizDTO dto)
        {
            var (success, message, data) = await _attemptService.SubmitQuizAsync(dto, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<QuizResultDTO>.Fail(message));

            return Ok(ApiResponse<QuizResultDTO>.Ok(data!, message));
        }

        // GET api/quizattempt/my-results
        // Get all quiz results for the logged-in user
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var results = await _attemptService.GetUserResultsAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<QuizResultDTO>>.Ok(results));
        }
    }
}

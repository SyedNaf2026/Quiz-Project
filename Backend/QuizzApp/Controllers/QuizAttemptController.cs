using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;

namespace QuizzApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitQuizDTO dto)
        {
            var (success, message, data) = await _attemptService.SubmitQuizAsync(dto, GetUserId());
            if (!success) return BadRequest(ApiResponse<QuizResultDTO>.Fail(message));
            return Ok(ApiResponse<QuizResultDTO>.Ok(data!, message));
        }

        // GET api/quizattempt/my-results
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var results = await _attemptService.GetUserResultsAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<QuizResultDTO>>.Ok(results));
        }

        // GET api/quizattempt/review/{quizId}
        // Returns full result with answer breakdown for review
        [HttpGet("review/{quizId}")]
        public async Task<IActionResult> ReviewResult(int quizId)
        {
            var result = await _attemptService.GetResultByQuizAsync(quizId, GetUserId());
            if (result == null) return NotFound(ApiResponse<QuizResultDTO>.Fail("Result not found."));
            return Ok(ApiResponse<QuizResultDTO>.Ok(result));
        }
    }
}

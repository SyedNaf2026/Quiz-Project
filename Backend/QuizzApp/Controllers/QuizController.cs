using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Services;

namespace QuizzApp.Controllers
{
    // QuizController handles all quiz CRUD for QuizCreators,
    // and browsing for QuizTakers
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // Helper: get the logged-in user's Id from the JWT claims
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // GET api/quiz?categoryId=1
        // Browse all active quizzes (QuizTaker + QuizCreator)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetActiveQuizzes([FromQuery] int? categoryId = null)
        {
            var quizzes = await _quizService.GetActiveQuizzesAsync(categoryId);
            return Ok(ApiResponse<IEnumerable<QuizDTO>>.Ok(quizzes));
        }

        // GET api/quiz/{id}
        // Get a single quiz by Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null)
                return NotFound(ApiResponse<QuizDTO>.Fail("Quiz not found."));

            return Ok(ApiResponse<QuizDTO>.Ok(quiz));
        }

        // GET api/quiz/my-quizzes
        // Get quizzes created by the logged-in QuizCreator
        [HttpGet("my-quizzes")]
        [Authorize(Roles = "QuizCreator")]
        public async Task<IActionResult> GetMyQuizzes()
        {
            var quizzes = await _quizService.GetQuizzesByCreatorAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<QuizDTO>>.Ok(quizzes));
        }

        // POST api/quiz
        // Create a new quiz (QuizCreator or GroupManager)
        [HttpPost]
        [Authorize(Roles = "QuizCreator,GroupManager")]
        public async Task<IActionResult> Create([FromBody] CreateQuizDTO dto)
        {
            //var (success, message, data) = await _quizService.CreateQuizAsync(dto,2);
            var (success, message, data) = await _quizService.CreateQuizAsync(dto, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<QuizDTO>.Fail(message));

            return Ok(ApiResponse<QuizDTO>.Ok(data!, message));
        }

        // PUT api/quiz/{id}
        // Update an existing quiz (QuizCreator only, must be creator)
        [HttpPut("{id}")]
        [Authorize(Roles = "QuizCreator")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuizDTO dto)
        {
            var (success, message) = await _quizService.UpdateQuizAsync(id, dto, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));

            return Ok(ApiResponse<string>.Ok("Updated.", message));
        }

        // DELETE api/quiz/{id}
        // Delete a quiz (QuizCreator only, must be creator)
        [HttpDelete("{id}")]
        //[Authorize(Roles = "QuizCreator")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _quizService.DeleteQuizAsync(id, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));

            return Ok(ApiResponse<string>.Ok("Deleted.", message));
        }

        // PATCH api/quiz/{id}/toggle-status
        // Activate or deactivate a quiz
        [HttpPatch("{id}/toggle-status")]
        //[Authorize(Roles = "QuizCreator")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var (success, message) = await _quizService.ToggleQuizStatusAsync(id, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));

            return Ok(ApiResponse<string>.Ok("Status changed.", message));
        }

        // GET api/quiz/{id}/stats
        // Get statistics for a quiz (QuizCreator only)
        [HttpGet("{id}/stats")]
        //[Authorize(Roles = "QuizCreator")]
        public async Task<IActionResult> GetStats(int id)
        {
            var stats = await _quizService.GetQuizStatsAsync(id, GetUserId());
            return Ok(ApiResponse<object>.Ok(stats));
        }
    }
}

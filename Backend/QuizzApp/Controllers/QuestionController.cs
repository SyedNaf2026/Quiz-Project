using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;

namespace QuizzApp.Controllers
{
    // QuestionController handles adding and managing questions in a quiz
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // GET api/question/quiz/{quizId}
        // Get all questions for a quiz (with options)
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetByQuiz(int quizId)
        {
            var questions = await _questionService.GetQuestionsByQuizAsync(quizId);
            return Ok(ApiResponse<IEnumerable<QuestionDTO>>.Ok(questions));
        }

        // POST api/question
        // Add a new question with options to a quiz
        [HttpPost]
        public async Task<IActionResult> AddQuestion([FromBody] CreateQuestionDTO dto)
        {
            var (success, message, data) = await _questionService.AddQuestionAsync(dto, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<QuestionDTO>.Fail(message));

            return Ok(ApiResponse<QuestionDTO>.Ok(data!, message));
        }

        // PUT api/question/{id}
        // Update a question's text
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] string newText)
        {
            var (success, message) = await _questionService.UpdateQuestionAsync(id, newText, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));

            return Ok(ApiResponse<string>.Ok("Updated.", message));
        }

        // DELETE api/question/{id}
        // Delete a question (options will be deleted too via cascade)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var (success, message) = await _questionService.DeleteQuestionAsync(id, GetUserId());

            if (!success)
                return BadRequest(ApiResponse<string>.Fail(message));

            return Ok(ApiResponse<string>.Ok("Deleted.", message));
        }
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Controllers
{
    // QuestionController handles adding and managing questions in a quiz
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly AppDbContext _context;

        public QuestionController(IQuestionService questionService, AppDbContext context)
        {
            _questionService = questionService;
            _context = context;
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
        [Authorize(Roles = "QuizCreator,GroupManager")]
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

        // POST api/question/bulk/{quizId}
        // Bulk upload questions from an Excel file
        [HttpPost("bulk/{quizId}")]
        [Authorize(Roles = "QuizCreator,GroupManager")]
        public async Task<IActionResult> UploadExcel(int quizId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.Fail("No file uploaded."));

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx")
                return BadRequest(ApiResponse<string>.Fail("Only .xlsx files are supported."));

            // EPPlus 5+ requires license context for non-commercial use
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            int added = 0;
            var errors = new List<string>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null)
                return BadRequest(ApiResponse<string>.Fail("Excel file has no worksheets."));

            // Row 1 = header, data starts at row 2
            // Columns: QuestionText | QuestionType | Opt1Text | Opt1Correct | Opt2Text | Opt2Correct | Opt3Text | Opt3Correct | Opt4Text | Opt4Correct
            for (int row = 2; row <= sheet.Dimension?.End.Row; row++)
            {
                var questionText = sheet.Cells[row, 1].Text.Trim();
                var questionType = sheet.Cells[row, 2].Text.Trim();

                if (string.IsNullOrEmpty(questionText)) continue;

                if (string.IsNullOrEmpty(questionType))
                    questionType = "MultipleChoice";

                var validTypes = new[] { "MultipleChoice", "MultipleAnswer", "TrueFalse", "YesNo" };
                if (!validTypes.Contains(questionType))
                {
                    errors.Add($"Row {row}: Invalid question type '{questionType}'.");
                    continue;
                }

                var options = new List<Option>();
                for (int col = 3; col <= 10; col += 2)
                {
                    var optText = sheet.Cells[row, col].Text.Trim();
                    if (string.IsNullOrEmpty(optText)) break;
                    var isCorrectText = sheet.Cells[row, col + 1].Text.Trim().ToLower();
                    var isCorrect = isCorrectText == "true" || isCorrectText == "yes" || isCorrectText == "1";
                    options.Add(new Option { OptionText = optText, IsCorrect = isCorrect });
                }

                if (options.Count < 2)
                {
                    errors.Add($"Row {row}: At least 2 options required.");
                    continue;
                }

                if (!options.Any(o => o.IsCorrect))
                {
                    errors.Add($"Row {row}: At least one correct option required.");
                    continue;
                }

                var question = new Question
                {
                    QuizId = quizId,
                    QuestionText = questionText,
                    QuestionType = questionType
                };
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                foreach (var opt in options)
                {
                    opt.QuestionId = question.Id;
                    _context.Options.Add(opt);
                }
                await _context.SaveChangesAsync();
                added++;
            }

            var msg = $"{added} question(s) imported successfully.";
            if (errors.Any()) msg += $" {errors.Count} row(s) skipped.";
            return Ok(ApiResponse<string>.Ok(msg));
        }
    }
}

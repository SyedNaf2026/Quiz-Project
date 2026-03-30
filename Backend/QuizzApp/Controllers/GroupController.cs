using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;

namespace QuizzApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // ── Group CRUD ────────────────────────────────────────────────

        // POST api/group
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupDTO dto)
        {
            var (success, message, data) = await _groupService.CreateGroupAsync(dto, GetUserId());
            if (!success) return BadRequest(ApiResponse<GroupDTO>.Fail(message));
            return Ok(ApiResponse<GroupDTO>.Ok(data!, message));
        }

        // GET api/group
        [HttpGet]
        public async Task<IActionResult> GetMyGroups()
        {
            var groups = await _groupService.GetMyGroupsAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<GroupDTO>>.Ok(groups));
        }

        // GET api/group/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id, GetUserId());
            if (group == null) return NotFound(ApiResponse<GroupDTO>.Fail("Group not found."));
            return Ok(ApiResponse<GroupDTO>.Ok(group));
        }

        // DELETE api/group/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _groupService.DeleteGroupAsync(id, GetUserId());
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }

        // ── Member Management ─────────────────────────────────────────

        // GET api/group/search-users?query=john
        [HttpGet("search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(ApiResponse<IEnumerable<UserSearchDTO>>.Ok(new List<UserSearchDTO>()));

            var users = await _groupService.SearchUsersAsync(query);
            return Ok(ApiResponse<IEnumerable<UserSearchDTO>>.Ok(users));
        }

        // GET api/group/{id}/members
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(int id)
        {
            var members = await _groupService.GetMembersAsync(id, GetUserId());
            return Ok(ApiResponse<IEnumerable<GroupMemberDTO>>.Ok(members));
        }

        // POST api/group/{id}/members/{userId}
        [HttpPost("{id}/members/{userId}")]
        public async Task<IActionResult> AddMember(int id, int userId)
        {
            var (success, message) = await _groupService.AddMemberAsync(id, userId, GetUserId());
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }

        // DELETE api/group/{id}/members/{userId}
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        {
            var (success, message) = await _groupService.RemoveMemberAsync(id, userId, GetUserId());
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }

        // ── Quiz Assignment ───────────────────────────────────────────

        // GET api/group/{id}/quizzes
        [HttpGet("{id}/quizzes")]
        public async Task<IActionResult> GetGroupQuizzes(int id)
        {
            var quizzes = await _groupService.GetGroupQuizzesAsync(id, GetUserId());
            return Ok(ApiResponse<IEnumerable<GroupQuizDTO>>.Ok(quizzes));
        }

        // POST api/group/{id}/quizzes/{quizId}
        [HttpPost("{id}/quizzes/{quizId}")]
        public async Task<IActionResult> AssignQuiz(int id, int quizId)
        {
            var (success, message) = await _groupService.AssignQuizAsync(id, quizId, GetUserId());
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }

        // DELETE api/group/{id}/quizzes/{quizId}
        [HttpDelete("{id}/quizzes/{quizId}")]
        public async Task<IActionResult> RemoveQuiz(int id, int quizId)
        {
            var (success, message) = await _groupService.RemoveQuizAsync(id, quizId, GetUserId());
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }

        // ── Submissions & Validation ──────────────────────────────────

        // GET api/group/{id}/submissions
        [HttpGet("{id}/submissions")]
        public async Task<IActionResult> GetSubmissions(int id)
        {
            var submissions = await _groupService.GetGroupSubmissionsAsync(id, GetUserId());
            return Ok(ApiResponse<IEnumerable<GroupQuizResultDTO>>.Ok(submissions));
        }

        // PUT api/group/submissions/{resultId}/validate
        [HttpPut("submissions/{resultId}/validate")]
        public async Task<IActionResult> Validate(int resultId, [FromBody] ValidateDTO dto)
        {
            var (success, message) = await _groupService.ValidateSubmissionAsync(resultId, dto.Status, GetUserId());
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }

        // GET api/group/my-results  — for QuizTaker
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyGroupResults()
        {
            var results = await _groupService.GetMyGroupResultsAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<GroupQuizResultDTO>>.Ok(results));
        }

        // GET api/group/my-quizzes  — for QuizTaker: quizzes assigned to their groups
        [HttpGet("my-quizzes")]
        public async Task<IActionResult> GetMyGroupQuizzes()
        {
            var quizzes = await _groupService.GetMyGroupQuizzesAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<GroupQuizDTO>>.Ok(quizzes));
        }

        // PUT api/group/{id}/quizzes/{quizId}/require-validation
        [HttpPut("{id}/quizzes/{quizId}/require-validation")]
        public async Task<IActionResult> SetRequiresValidation(int id, int quizId)
        {
            var (success, message) = await _groupService.SetRequiresValidationAsync(id, quizId, GetUserId());
            if (!success) return BadRequest(ApiResponse<string>.Fail(message));
            return Ok(ApiResponse<string>.Ok(message));
        }
        // GET api/group/completed-quiz-ids
        [HttpGet("completed-quiz-ids")]
        public async Task<IActionResult> GetCompletedGroupQuizIds()
        {
            var ids = await _groupService.GetCompletedGroupQuizIdsAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<int>>.Ok(ids));
        }
    }

    public class ValidateDTO
    {
        public string Status { get; set; } = string.Empty;
    }
}

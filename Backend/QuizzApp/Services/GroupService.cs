using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public GroupService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ── Group CRUD ────────────────────────────────────────────────

        public async Task<(bool Success, string Message, GroupDTO? Data)> CreateGroupAsync(CreateGroupDTO dto, int managerId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "Group name is required.", null);

            var group = new Group { Name = dto.Name, Description = dto.Description, CreatedBy = managerId };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return (true, "Group created successfully.", new GroupDTO
            {
                Id = group.Id, Name = group.Name, Description = group.Description,
                MemberCount = 0, QuizCount = 0, CreatedAt = group.CreatedAt
            });
        }

        public async Task<IEnumerable<GroupDTO>> GetMyGroupsAsync(int managerId)
        {
            return await _context.Groups
                .Where(g => g.CreatedBy == managerId)
                .Select(g => new GroupDTO
                {
                    Id = g.Id, Name = g.Name, Description = g.Description,
                    CreatorName = g.Creator != null ? g.Creator.FullName : "",
                    MemberCount = g.Members.Count, QuizCount = g.GroupQuizzes.Count, CreatedAt = g.CreatedAt
                }).ToListAsync();
        }

        public async Task<GroupDTO?> GetGroupByIdAsync(int groupId, int managerId)
        {
            var g = await _context.Groups
                .Include(g => g.Creator).Include(g => g.Members).Include(g => g.GroupQuizzes)
                .FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (g == null) return null;
            return new GroupDTO
            {
                Id = g.Id, Name = g.Name, Description = g.Description,
                CreatorName = g.Creator?.FullName ?? "",
                MemberCount = g.Members.Count, QuizCount = g.GroupQuizzes.Count, CreatedAt = g.CreatedAt
            };
        }

        public async Task<(bool Success, string Message)> DeleteGroupAsync(int groupId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return (false, "Group not found.");
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return (true, "Group deleted.");
        }

        // ── Member Management ─────────────────────────────────────────

        public async Task<IEnumerable<UserSearchDTO>> SearchUsersAsync(string query)
        {
            var lower = query.ToLower();
            return await _context.Users
                .Where(u => u.Role == "QuizTaker" &&
                    (u.FullName.ToLower().Contains(lower) || u.Email.ToLower().Contains(lower)))
                .Select(u => new UserSearchDTO { Id = u.Id, FullName = u.FullName, Email = u.Email })
                .Take(20).ToListAsync();
        }

        public async Task<(bool Success, string Message)> AddMemberAsync(int groupId, int userId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return (false, "Group not found.");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == "QuizTaker");
            if (user == null) return (false, "User not found.");
            var exists = await _context.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            if (exists) return (false, "User is already a member.");
            _context.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userId });
            await _context.SaveChangesAsync();
            return (true, "Member added.");
        }

        public async Task<(bool Success, string Message)> RemoveMemberAsync(int groupId, int userId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return (false, "Group not found.");
            var member = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            if (member == null) return (false, "Member not found.");
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            return (true, "Member removed.");
        }

        public async Task<IEnumerable<GroupMemberDTO>> GetMembersAsync(int groupId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return Enumerable.Empty<GroupMemberDTO>();
            return await _context.GroupMembers.Include(gm => gm.User).Where(gm => gm.GroupId == groupId)
                .Select(gm => new GroupMemberDTO
                {
                    Id = gm.Id, UserId = gm.UserId,
                    FullName = gm.User != null ? gm.User.FullName : "",
                    Email = gm.User != null ? gm.User.Email : "",
                    JoinedAt = gm.JoinedAt
                }).ToListAsync();
        }

        // ── Quiz Assignment ───────────────────────────────────────────

        public async Task<(bool Success, string Message)> AssignQuizAsync(int groupId, int quizId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return (false, "Group not found.");
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) return (false, "Quiz not found.");
            var exists = await _context.GroupQuizzes.AnyAsync(gq => gq.GroupId == groupId && gq.QuizId == quizId);
            if (exists) return (false, "Quiz already assigned to this group.");

            _context.GroupQuizzes.Add(new GroupQuiz { GroupId = groupId, QuizId = quizId });
            await _context.SaveChangesAsync();

            // Notify every member
            var memberIds = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId).Select(gm => gm.UserId).ToListAsync();
            foreach (var memberId in memberIds)
            {
                await _notificationService.SendToUserAsync(
                    memberId,
                    $"📝 New quiz assigned in group \"{group.Name}\": \"{quiz.Title}\"",
                    "group_quiz_assigned");
            }

            return (true, "Quiz assigned.");
        }

        public async Task<(bool Success, string Message)> RemoveQuizAsync(int groupId, int quizId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return (false, "Group not found.");
            var gq = await _context.GroupQuizzes.FirstOrDefaultAsync(gq => gq.GroupId == groupId && gq.QuizId == quizId);
            if (gq == null) return (false, "Quiz not assigned to this group.");
            _context.GroupQuizzes.Remove(gq);
            await _context.SaveChangesAsync();
            return (true, "Quiz removed from group.");
        }

        public async Task<IEnumerable<GroupQuizDTO>> GetGroupQuizzesAsync(int groupId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return Enumerable.Empty<GroupQuizDTO>();
            return await _context.GroupQuizzes
                .Include(gq => gq.Quiz).ThenInclude(q => q!.Category)
                .Include(gq => gq.Quiz).ThenInclude(q => q!.Questions)
                .Where(gq => gq.GroupId == groupId)
                .Select(gq => new GroupQuizDTO
                {
                    GroupQuizId = gq.Id, QuizId = gq.QuizId,
                    QuizTitle = gq.Quiz != null ? gq.Quiz.Title : "",
                    CategoryName = gq.Quiz != null && gq.Quiz.Category != null ? gq.Quiz.Category.Name : "",
                    TotalQuestions = gq.Quiz != null ? gq.Quiz.Questions.Count : 0,
                    AssignedAt = gq.AssignedAt,
                    RequiresValidation = gq.RequiresValidation
                }).ToListAsync();
        }

        // ── Submissions & Validation ──────────────────────────────────

        public async Task<IEnumerable<GroupQuizResultDTO>> GetGroupSubmissionsAsync(int groupId, int managerId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId && g.CreatedBy == managerId);
            if (group == null) return Enumerable.Empty<GroupQuizResultDTO>();
            return await _context.GroupQuizResults
                .Include(gr => gr.User)
                .Include(gr => gr.QuizResult).ThenInclude(qr => qr!.Quiz)
                .Include(gr => gr.GroupQuiz).ThenInclude(gq => gq!.Group)
                .Where(gr => gr.GroupQuiz!.GroupId == groupId)
                .Select(gr => new GroupQuizResultDTO
                {
                    Id = gr.Id, UserId = gr.UserId,
                    UserName = gr.User != null ? gr.User.FullName : "",
                    GroupName = gr.GroupQuiz != null && gr.GroupQuiz.Group != null ? gr.GroupQuiz.Group.Name : "",
                    QuizTitle = gr.QuizResult != null && gr.QuizResult.Quiz != null ? gr.QuizResult.Quiz.Title : "",
                    Score = gr.QuizResult != null ? gr.QuizResult.Score : 0,
                    TotalQuestions = gr.QuizResult != null ? gr.QuizResult.TotalQuestions : 0,
                    Percentage = gr.QuizResult != null ? gr.QuizResult.Percentage : 0,
                    ValidationStatus = gr.ValidationStatus,
                    RequiresValidation = gr.GroupQuiz != null && gr.GroupQuiz.RequiresValidation,
                    SubmittedAt = gr.SubmittedAt
                }).ToListAsync();
        }

        public async Task<(bool Success, string Message)> ValidateSubmissionAsync(int groupQuizResultId, string status, int managerId)
        {
            if (status != "Approved" && status != "Pending")
                return (false, "Status must be 'Approved' or 'Pending'.");
            var result = await _context.GroupQuizResults
                .Include(gr => gr.GroupQuiz).ThenInclude(gq => gq!.Group)
                .FirstOrDefaultAsync(gr => gr.Id == groupQuizResultId);
            if (result == null) return (false, "Submission not found.");
            if (result.GroupQuiz?.Group?.CreatedBy != managerId) return (false, "Access denied.");
            result.ValidationStatus = status;
            await _context.SaveChangesAsync();
            return (true, $"Submission marked as {status}.");
        }

        // ── QuizTaker: get their own group results ────────────────────

        public async Task<IEnumerable<GroupQuizResultDTO>> GetMyGroupResultsAsync(int userId)
        {
            return await _context.GroupQuizResults
                .Include(gr => gr.QuizResult).ThenInclude(qr => qr!.Quiz)
                .Include(gr => gr.GroupQuiz).ThenInclude(gq => gq!.Group)
                .Where(gr => gr.UserId == userId)
                .OrderByDescending(gr => gr.SubmittedAt)
                .Select(gr => new GroupQuizResultDTO
                {
                    Id = gr.Id, UserId = gr.UserId, UserName = "",
                    GroupName = gr.GroupQuiz != null && gr.GroupQuiz.Group != null ? gr.GroupQuiz.Group.Name : "",
                    QuizTitle = gr.QuizResult != null && gr.QuizResult.Quiz != null ? gr.QuizResult.Quiz.Title : "",
                    Score = gr.QuizResult != null ? gr.QuizResult.Score : 0,
                    TotalQuestions = gr.QuizResult != null ? gr.QuizResult.TotalQuestions : 0,
                    Percentage = gr.QuizResult != null ? gr.QuizResult.Percentage : 0,
                    ValidationStatus = gr.ValidationStatus,
                    RequiresValidation = gr.GroupQuiz != null && gr.GroupQuiz.RequiresValidation,
                    SubmittedAt = gr.SubmittedAt
                }).ToListAsync();
        }

        // ── QuizTaker: get quizzes assigned to their groups ───────────

        public async Task<IEnumerable<GroupQuizDTO>> GetMyGroupQuizzesAsync(int userId)
        {
            return await _context.GroupQuizzes
                .Include(gq => gq.Quiz).ThenInclude(q => q!.Category)
                .Include(gq => gq.Quiz).ThenInclude(q => q!.Questions)
                .Include(gq => gq.Group).ThenInclude(g => g!.Members)
                .Where(gq => gq.Group!.Members.Any(m => m.UserId == userId) && gq.Quiz!.IsActive)
                .Select(gq => new GroupQuizDTO
                {
                    GroupQuizId = gq.Id, QuizId = gq.QuizId,
                    QuizTitle = gq.Quiz != null ? gq.Quiz.Title : "",
                    CategoryName = gq.Quiz != null && gq.Quiz.Category != null ? gq.Quiz.Category.Name : "",
                    TotalQuestions = gq.Quiz != null ? gq.Quiz.Questions.Count : 0,
                    AssignedAt = gq.AssignedAt,
                    RequiresValidation = gq.RequiresValidation
                }).ToListAsync();
        }

        // ── Called after quiz submission — handled directly in QuizAttemptService ──

        public async Task<IEnumerable<int>> GetCompletedGroupQuizIdsAsync(int userId)
        {
            return await _context.GroupQuizResults
                .Include(gr => gr.GroupQuiz)
                .Where(gr => gr.UserId == userId)
                .Select(gr => gr.GroupQuiz!.QuizId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> SetRequiresValidationAsync(int groupId, int quizId, int managerId)
        {
            var gq = await _context.GroupQuizzes
                .Include(gq => gq.Group)
                .FirstOrDefaultAsync(gq => gq.GroupId == groupId && gq.QuizId == quizId && gq.Group!.CreatedBy == managerId);
            if (gq == null) return (false, "Group quiz not found.");
            gq.RequiresValidation = true;
            await _context.SaveChangesAsync();
            return (true, "Validation required.");
        }
    }
}

using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface IGroupService
    {
        // Group CRUD
        Task<(bool Success, string Message, GroupDTO? Data)> CreateGroupAsync(CreateGroupDTO dto, int managerId);
        Task<IEnumerable<GroupDTO>> GetMyGroupsAsync(int managerId);
        Task<GroupDTO?> GetGroupByIdAsync(int groupId, int managerId);
        Task<(bool Success, string Message)> DeleteGroupAsync(int groupId, int managerId);

        // Member management
        Task<IEnumerable<UserSearchDTO>> SearchUsersAsync(string query);
        Task<(bool Success, string Message)> AddMemberAsync(int groupId, int userId, int managerId);
        Task<(bool Success, string Message)> RemoveMemberAsync(int groupId, int userId, int managerId);
        Task<IEnumerable<GroupMemberDTO>> GetMembersAsync(int groupId, int managerId);

        // Quiz assignment
        Task<(bool Success, string Message)> AssignQuizAsync(int groupId, int quizId, int managerId);
        Task<(bool Success, string Message)> RemoveQuizAsync(int groupId, int quizId, int managerId);
        Task<IEnumerable<GroupQuizDTO>> GetGroupQuizzesAsync(int groupId, int managerId);

        // Submissions & validation
        Task<IEnumerable<GroupQuizResultDTO>> GetGroupSubmissionsAsync(int groupId, int managerId);
        Task<(bool Success, string Message)> ValidateSubmissionAsync(int groupQuizResultId, string status, int managerId);

        // For QuizTaker: get their group results
        Task<IEnumerable<GroupQuizResultDTO>> GetMyGroupResultsAsync(int userId);

        // For QuizTaker: get quizzes assigned to their groups
        Task<IEnumerable<GroupQuizDTO>> GetMyGroupQuizzesAsync(int userId);

        // For QuizTaker: get quiz IDs already completed in their group
        Task<IEnumerable<int>> GetCompletedGroupQuizIdsAsync(int userId);

        // Mark a group quiz as requiring validation (called after GM creates a quiz)
        Task<(bool Success, string Message)> SetRequiresValidationAsync(int groupId, int quizId, int managerId);
    }
}

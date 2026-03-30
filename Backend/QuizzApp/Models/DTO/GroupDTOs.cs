namespace QuizzApp.DTOs
{
    public class CreateGroupDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class GroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int QuizCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GroupMemberDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class GroupQuizDTO
    {
        public int GroupQuizId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool RequiresValidation { get; set; }
    }

    public class GroupQuizResultDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public string ValidationStatus { get; set; } = "Pending";
        public bool RequiresValidation { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class UserSearchDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}

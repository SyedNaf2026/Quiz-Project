using Microsoft.EntityFrameworkCore;
using Moq;
using QuizzApp.Context;
using QuizzApp.Interfaces;
using QuizzApp.Models;
using QuizzApp.Services;
using QuizzApp.DTOs;
using Xunit;

namespace QuizzApp.Tests
{
    public class GroupServiceTests
    {
        private readonly Mock<INotificationService> _notifMock;

        public GroupServiceTests()
        {
            _notifMock = new Mock<INotificationService>();
            _notifMock.Setup(n => n.SendToUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);
            _notifMock.Setup(n => n.SendToAllTakersAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .Returns(Task.CompletedTask);
        }

        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private GroupService CreateService(AppDbContext db) =>
            new GroupService(db, _notifMock.Object);

        // ── Seed helpers ──────────────────────────────────────────────

        private async Task<User> SeedManager(AppDbContext db, int id = 1)
        {
            var user = new User { Id = id, FullName = "Manager", Email = $"mgr{id}@test.com", Role = "GroupManager" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        private async Task<User> SeedTaker(AppDbContext db, int id = 10)
        {
            var user = new User { Id = id, FullName = "Taker", Email = $"taker{id}@test.com", Role = "QuizTaker" };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        private async Task<Group> SeedGroup(AppDbContext db, int managerId = 1)
        {
            var group = new Group { Name = "Test Group", Description = "Desc", CreatedBy = managerId };
            db.Groups.Add(group);
            await db.SaveChangesAsync();
            return group;
        }

        private async Task<Quiz> SeedQuiz(AppDbContext db, int creatorId = 1)
        {
            var cat = new Category { Id = 1, Name = "General" };
            db.Categories.Add(cat);
            var quiz = new Quiz { Title = "Test Quiz", CategoryId = 1, CreatedBy = creatorId, IsActive = true };
            db.Quizzes.Add(quiz);
            await db.SaveChangesAsync();
            return quiz;
        }

        // ── CreateGroup ───────────────────────────────────────────────

        [Fact]
        public async Task CreateGroup_EmptyName_ReturnsFail()
        {
            using var db = CreateDb("GS_Create_EmptyName");
            var service = CreateService(db);

            var (success, message, data) = await service.CreateGroupAsync(new CreateGroupDTO { Name = "" }, 1);

            Assert.False(success);
            Assert.Equal("Group name is required.", message);
            Assert.Null(data);
        }

        [Fact]
        public async Task CreateGroup_ValidData_ReturnsSuccess()
        {
            using var db = CreateDb("GS_Create_Valid");
            var service = CreateService(db);

            var (success, message, data) = await service.CreateGroupAsync(
                new CreateGroupDTO { Name = "Alpha", Description = "Test" }, managerId: 1);

            Assert.True(success);
            Assert.NotNull(data);
            Assert.Equal("Alpha", data.Name);
            Assert.Equal(0, data.MemberCount);
        }

        // ── GetMyGroups ───────────────────────────────────────────────

        [Fact]
        public async Task GetMyGroups_ReturnsOnlyManagersGroups()
        {
            using var db = CreateDb("GS_GetMyGroups");
            db.Groups.AddRange(
                new Group { Name = "G1", CreatedBy = 1 },
                new Group { Name = "G2", CreatedBy = 2 }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.GetMyGroupsAsync(managerId: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("G1", result[0].Name);
        }

        // ── DeleteGroup ───────────────────────────────────────────────

        [Fact]
        public async Task DeleteGroup_NotFound_ReturnsFail()
        {
            using var db = CreateDb("GS_Delete_NotFound");
            var service = CreateService(db);

            var (success, message) = await service.DeleteGroupAsync(99, managerId: 1);

            Assert.False(success);
            Assert.Equal("Group not found.", message);
        }

        [Fact]
        public async Task DeleteGroup_Valid_ReturnsSuccess()
        {
            using var db = CreateDb("GS_Delete_Valid");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var service = CreateService(db);

            var (success, _) = await service.DeleteGroupAsync(group.Id, managerId: 1);

            Assert.True(success);
            Assert.Empty(db.Groups);
        }

        // ── SearchUsers ───────────────────────────────────────────────

        [Fact]
        public async Task SearchUsers_ReturnsOnlyQuizTakers()
        {
            using var db = CreateDb("GS_Search");
            db.Users.AddRange(
                new User { FullName = "Alice Taker", Email = "alice@test.com", Role = "QuizTaker" },
                new User { FullName = "Bob Creator", Email = "bob@test.com", Role = "QuizCreator" }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.SearchUsersAsync("alice")).ToList();

            Assert.Single(result);
            Assert.Equal("Alice Taker", result[0].FullName);
        }

        [Fact]
        public async Task SearchUsers_MatchesByEmail()
        {
            using var db = CreateDb("GS_Search_Email");
            db.Users.Add(new User { FullName = "Charlie", Email = "charlie@quiz.com", Role = "QuizTaker" });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.SearchUsersAsync("charlie@quiz")).ToList();

            Assert.Single(result);
        }

        // ── AddMember ─────────────────────────────────────────────────

        [Fact]
        public async Task AddMember_GroupNotFound_ReturnsFail()
        {
            using var db = CreateDb("GS_AddMember_NoGroup");
            var service = CreateService(db);

            var (success, message) = await service.AddMemberAsync(99, 10, managerId: 1);

            Assert.False(success);
            Assert.Equal("Group not found.", message);
        }

        [Fact]
        public async Task AddMember_UserNotQuizTaker_ReturnsFail()
        {
            using var db = CreateDb("GS_AddMember_NotTaker");
            await SeedManager(db);
            var group = await SeedGroup(db);
            db.Users.Add(new User { Id = 10, FullName = "Creator", Email = "c@test.com", Role = "QuizCreator" });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, message) = await service.AddMemberAsync(group.Id, 10, managerId: 1);

            Assert.False(success);
            Assert.Equal("User not found.", message);
        }

        [Fact]
        public async Task AddMember_AlreadyMember_ReturnsFail()
        {
            using var db = CreateDb("GS_AddMember_Duplicate");
            await SeedManager(db);
            var group = await SeedGroup(db);
            await SeedTaker(db);
            db.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = 10 });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, message) = await service.AddMemberAsync(group.Id, 10, managerId: 1);

            Assert.False(success);
            Assert.Equal("User is already a member.", message);
        }

        [Fact]
        public async Task AddMember_Valid_ReturnsSuccess()
        {
            using var db = CreateDb("GS_AddMember_Valid");
            await SeedManager(db);
            var group = await SeedGroup(db);
            await SeedTaker(db);
            var service = CreateService(db);

            var (success, _) = await service.AddMemberAsync(group.Id, 10, managerId: 1);

            Assert.True(success);
            Assert.Single(db.GroupMembers);
        }

        // ── RemoveMember ──────────────────────────────────────────────

        [Fact]
        public async Task RemoveMember_NotFound_ReturnsFail()
        {
            using var db = CreateDb("GS_RemoveMember_NotFound");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var service = CreateService(db);

            var (success, message) = await service.RemoveMemberAsync(group.Id, 99, managerId: 1);

            Assert.False(success);
            Assert.Equal("Member not found.", message);
        }

        [Fact]
        public async Task RemoveMember_Valid_ReturnsSuccess()
        {
            using var db = CreateDb("GS_RemoveMember_Valid");
            await SeedManager(db);
            var group = await SeedGroup(db);
            await SeedTaker(db);
            db.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = 10 });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, _) = await service.RemoveMemberAsync(group.Id, 10, managerId: 1);

            Assert.True(success);
            Assert.Empty(db.GroupMembers);
        }

        // ── AssignQuiz ────────────────────────────────────────────────

        [Fact]
        public async Task AssignQuiz_GroupNotFound_ReturnsFail()
        {
            using var db = CreateDb("GS_AssignQuiz_NoGroup");
            var service = CreateService(db);

            var (success, message) = await service.AssignQuizAsync(99, 1, managerId: 1);

            Assert.False(success);
            Assert.Equal("Group not found.", message);
        }

        [Fact]
        public async Task AssignQuiz_AlreadyAssigned_ReturnsFail()
        {
            using var db = CreateDb("GS_AssignQuiz_Duplicate");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var quiz = await SeedQuiz(db);
            db.GroupQuizzes.Add(new GroupQuiz { GroupId = group.Id, QuizId = quiz.Id });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, message) = await service.AssignQuizAsync(group.Id, quiz.Id, managerId: 1);

            Assert.False(success);
            Assert.Equal("Quiz already assigned to this group.", message);
        }

        [Fact]
        public async Task AssignQuiz_Valid_NotifiesMembersAndReturnsSuccess()
        {
            using var db = CreateDb("GS_AssignQuiz_Valid");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var quiz = await SeedQuiz(db);
            await SeedTaker(db);
            db.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = 10 });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, _) = await service.AssignQuizAsync(group.Id, quiz.Id, managerId: 1);

            Assert.True(success);
            Assert.Single(db.GroupQuizzes);
            // Notification sent to the member
            _notifMock.Verify(n => n.SendToUserAsync(10, It.IsAny<string>(), "group_quiz_assigned"), Times.Once);
        }

        // ── RemoveQuiz ────────────────────────────────────────────────

        [Fact]
        public async Task RemoveQuiz_NotAssigned_ReturnsFail()
        {
            using var db = CreateDb("GS_RemoveQuiz_NotAssigned");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var service = CreateService(db);

            var (success, message) = await service.RemoveQuizAsync(group.Id, 99, managerId: 1);

            Assert.False(success);
            Assert.Equal("Quiz not assigned to this group.", message);
        }

        [Fact]
        public async Task RemoveQuiz_Valid_ReturnsSuccess()
        {
            using var db = CreateDb("GS_RemoveQuiz_Valid");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var quiz = await SeedQuiz(db);
            db.GroupQuizzes.Add(new GroupQuiz { GroupId = group.Id, QuizId = quiz.Id });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, _) = await service.RemoveQuizAsync(group.Id, quiz.Id, managerId: 1);

            Assert.True(success);
            Assert.Empty(db.GroupQuizzes);
        }

        // ── ValidateSubmission ────────────────────────────────────────

        [Fact]
        public async Task ValidateSubmission_InvalidStatus_ReturnsFail()
        {
            using var db = CreateDb("GS_Validate_BadStatus");
            var service = CreateService(db);

            var (success, message) = await service.ValidateSubmissionAsync(1, "Invalid", managerId: 1);

            Assert.False(success);
            Assert.Equal("Status must be 'Approved' or 'Pending'.", message);
        }

        [Fact]
        public async Task ValidateSubmission_NotFound_ReturnsFail()
        {
            using var db = CreateDb("GS_Validate_NotFound");
            var service = CreateService(db);

            var (success, message) = await service.ValidateSubmissionAsync(99, "Approved", managerId: 1);

            Assert.False(success);
            Assert.Equal("Submission not found.", message);
        }

        [Fact]
        public async Task ValidateSubmission_Valid_UpdatesStatus()
        {
            using var db = CreateDb("GS_Validate_Valid");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var quiz = await SeedQuiz(db);
            await SeedTaker(db);
            var gq = new GroupQuiz { GroupId = group.Id, QuizId = quiz.Id, RequiresValidation = true };
            db.GroupQuizzes.Add(gq);
            await db.SaveChangesAsync();

            var qr = new QuizResult { UserId = 10, QuizId = quiz.Id, Score = 3, TotalQuestions = 5, Percentage = 60 };
            db.QuizResults.Add(qr);
            await db.SaveChangesAsync();

            var gqr = new GroupQuizResult { GroupQuizId = gq.Id, UserId = 10, QuizResultId = qr.Id, ValidationStatus = "Pending" };
            db.GroupQuizResults.Add(gqr);
            await db.SaveChangesAsync();

            var service = CreateService(db);

            var (success, _) = await service.ValidateSubmissionAsync(gqr.Id, "Approved", managerId: 1);

            Assert.True(success);
            Assert.Equal("Approved", db.GroupQuizResults.First().ValidationStatus);
        }

        // ── SetRequiresValidation ─────────────────────────────────────

        [Fact]
        public async Task SetRequiresValidation_Valid_SetsFlag()
        {
            using var db = CreateDb("GS_SetValidation");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var quiz = await SeedQuiz(db);
            db.GroupQuizzes.Add(new GroupQuiz { GroupId = group.Id, QuizId = quiz.Id, RequiresValidation = false });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var (success, _) = await service.SetRequiresValidationAsync(group.Id, quiz.Id, managerId: 1);

            Assert.True(success);
            Assert.True(db.GroupQuizzes.First().RequiresValidation);
        }

        // ── GetMyGroupResults ─────────────────────────────────────────

        [Fact]
        public async Task GetMyGroupResults_ReturnsOnlyUserResults()
        {
            using var db = CreateDb("GS_MyGroupResults");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var quiz = await SeedQuiz(db);
            await SeedTaker(db, 10);
            await SeedTaker(db, 11);

            var gq = new GroupQuiz { GroupId = group.Id, QuizId = quiz.Id };
            db.GroupQuizzes.Add(gq);
            await db.SaveChangesAsync();

            var qr1 = new QuizResult { UserId = 10, QuizId = quiz.Id, Score = 4, TotalQuestions = 5, Percentage = 80 };
            var qr2 = new QuizResult { UserId = 11, QuizId = quiz.Id, Score = 2, TotalQuestions = 5, Percentage = 40 };
            db.QuizResults.AddRange(qr1, qr2);
            await db.SaveChangesAsync();

            db.GroupQuizResults.AddRange(
                new GroupQuizResult { GroupQuizId = gq.Id, UserId = 10, QuizResultId = qr1.Id },
                new GroupQuizResult { GroupQuizId = gq.Id, UserId = 11, QuizResultId = qr2.Id }
            );
            await db.SaveChangesAsync();

            var service = CreateService(db);

            var result = (await service.GetMyGroupResultsAsync(userId: 10)).ToList();

            Assert.Single(result);
            Assert.Equal(80, result[0].Percentage);
        }

        // ── GetCompletedGroupQuizIds ──────────────────────────────────

        [Fact]
        public async Task GetCompletedGroupQuizIds_ReturnsCorrectIds()
        {
            using var db = CreateDb("GS_CompletedIds");
            await SeedManager(db);
            var group = await SeedGroup(db);
            var quiz = await SeedQuiz(db);
            await SeedTaker(db);

            var gq = new GroupQuiz { GroupId = group.Id, QuizId = quiz.Id };
            db.GroupQuizzes.Add(gq);
            await db.SaveChangesAsync();

            var qr = new QuizResult { UserId = 10, QuizId = quiz.Id, Score = 3, TotalQuestions = 5, Percentage = 60 };
            db.QuizResults.Add(qr);
            await db.SaveChangesAsync();

            db.GroupQuizResults.Add(new GroupQuizResult { GroupQuizId = gq.Id, UserId = 10, QuizResultId = qr.Id });
            await db.SaveChangesAsync();

            var service = CreateService(db);

            var ids = (await service.GetCompletedGroupQuizIdsAsync(userId: 10)).ToList();

            Assert.Single(ids);
            Assert.Equal(quiz.Id, ids[0]);
        }
    }
}

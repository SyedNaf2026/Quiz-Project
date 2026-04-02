using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuizzApp.Context;
using QuizzApp.Hubs;
using QuizzApp.Models;
using QuizzApp.Services;
using Xunit;

namespace QuizzApp.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IHubContext<NotificationHub>> _hubMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IHubClients> _hubClientsMock;

        public NotificationServiceTests()
        {
            _clientProxyMock = new Mock<IClientProxy>();
            _clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _hubClientsMock = new Mock<IHubClients>();
            _hubClientsMock.Setup(h => h.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);

            _hubMock = new Mock<IHubContext<NotificationHub>>();
            _hubMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
        }

        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private NotificationService CreateService(AppDbContext db) =>
            new NotificationService(db, _hubMock.Object);

        // ── SendToUserAsync ───────────────────────────────────────────

        [Fact]
        public async Task SendToUser_PersistsNotificationToDb()
        {
            using var db = CreateDb("NS_SendToUser_Persist");
            var service = CreateService(db);

            await service.SendToUserAsync(1, "Test message", "quiz_added");

            Assert.Single(db.Notifications);
            var n = db.Notifications.First();
            Assert.Equal(1, n.UserId);
            Assert.Equal("Test message", n.Message);
            Assert.Equal("quiz_added", n.Type);
            Assert.False(n.IsRead);
        }

        [Fact]
        public async Task SendToUser_PushesSignalRNotification()
        {
            using var db = CreateDb("NS_SendToUser_SignalR");
            var service = CreateService(db);

            await service.SendToUserAsync(5, "Hello", "quiz_updated");

            _hubClientsMock.Verify(h => h.Group("user_5"), Times.Once);
            _clientProxyMock.Verify(c => c.SendCoreAsync(
                "ReceiveNotification", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // ── SendToAllTakersAsync ──────────────────────────────────────

        [Fact]
        public async Task SendToAllTakers_NotifiesEachTaker()
        {
            using var db = CreateDb("NS_SendToAll");
            db.Users.AddRange(
                new User { Id = 1, FullName = "T1", Email = "t1@test.com", Role = "QuizTaker" },
                new User { Id = 2, FullName = "T2", Email = "t2@test.com", Role = "QuizTaker" },
                new User { Id = 3, FullName = "C1", Email = "c1@test.com", Role = "QuizCreator" }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            await service.SendToAllTakersAsync("New quiz!", "quiz_added");

            // 2 notifications saved (only QuizTakers)
            Assert.Equal(2, db.Notifications.Count());
            // SignalR pushed to 2 groups
            _hubClientsMock.Verify(h => h.Group(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SendToAllTakers_OnlyTargetsQuizTakers()
        {
            using var db = CreateDb("NS_SendToAll_RoleFilter");
            db.Users.AddRange(
                new User { Id = 1, FullName = "Taker", Email = "t@test.com", Role = "QuizTaker" },
                new User { Id = 2, FullName = "Manager", Email = "m@test.com", Role = "GroupManager" },
                new User { Id = 3, FullName = "Admin", Email = "a@test.com", Role = "Admin" }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            await service.SendToAllTakersAsync("Message", "quiz_added");

            // Only 1 notification — only the QuizTaker
            Assert.Single(db.Notifications);
            Assert.Equal(1, db.Notifications.First().UserId);
        }

        // ── GetUserNotificationsAsync ─────────────────────────────────

        [Fact]
        public async Task GetUserNotifications_ReturnsOnlyUserNotifications()
        {
            using var db = CreateDb("NS_GetNotifs");
            db.Notifications.AddRange(
                new Notification { UserId = 1, Message = "For user 1", Type = "quiz_added" },
                new Notification { UserId = 2, Message = "For user 2", Type = "quiz_added" }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.GetUserNotificationsAsync(userId: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("For user 1", result[0].Message);
        }

        [Fact]
        public async Task GetUserNotifications_OrderedByNewestFirst()
        {
            using var db = CreateDb("NS_GetNotifs_Order");
            db.Notifications.AddRange(
                new Notification { UserId = 1, Message = "Old", Type = "quiz_added", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Notification { UserId = 1, Message = "New", Type = "quiz_added", CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = (await service.GetUserNotificationsAsync(userId: 1)).ToList();

            Assert.Equal("New", result[0].Message);
            Assert.Equal("Old", result[1].Message);
        }

        [Fact]
        public async Task GetUserNotifications_WhenNone_ReturnsEmpty()
        {
            using var db = CreateDb("NS_GetNotifs_Empty");
            var service = CreateService(db);

            var result = await service.GetUserNotificationsAsync(userId: 99);

            Assert.Empty(result);
        }

        // ── MarkReadAsync ─────────────────────────────────────────────

        [Fact]
        public async Task MarkRead_SetsIsReadTrue()
        {
            using var db = CreateDb("NS_MarkRead");
            db.Notifications.Add(new Notification { Id = 1, UserId = 1, Message = "Test", Type = "quiz_added", IsRead = false });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            await service.MarkReadAsync(notificationId: 1, userId: 1);

            Assert.True(db.Notifications.First().IsRead);
        }

        [Fact]
        public async Task MarkRead_WrongUser_DoesNotMark()
        {
            using var db = CreateDb("NS_MarkRead_WrongUser");
            db.Notifications.Add(new Notification { Id = 1, UserId = 1, Message = "Test", Type = "quiz_added", IsRead = false });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            // userId 2 tries to mark notification belonging to userId 1
            await service.MarkReadAsync(notificationId: 1, userId: 2);

            // Should remain unread
            Assert.False(db.Notifications.First().IsRead);
        }

        // ── MarkAllReadAsync ──────────────────────────────────────────

        [Fact]
        public async Task MarkAllRead_MarksAllUnreadForUser()
        {
            using var db = CreateDb("NS_MarkAllRead");
            db.Notifications.AddRange(
                new Notification { UserId = 1, Message = "N1", Type = "quiz_added", IsRead = false },
                new Notification { UserId = 1, Message = "N2", Type = "quiz_added", IsRead = false },
                new Notification { UserId = 2, Message = "N3", Type = "quiz_added", IsRead = false }
            );
            await db.SaveChangesAsync();
            var service = CreateService(db);

            await service.MarkAllReadAsync(userId: 1);

            var user1Notifs = db.Notifications.Where(n => n.UserId == 1).ToList();
            var user2Notifs = db.Notifications.Where(n => n.UserId == 2).ToList();

            Assert.All(user1Notifs, n => Assert.True(n.IsRead));
            Assert.All(user2Notifs, n => Assert.False(n.IsRead)); // user 2 unaffected
        }
    }
}

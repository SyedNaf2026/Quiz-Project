using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizzApp.Context;
using QuizzApp.DTOs;
using QuizzApp.Hubs;
using QuizzApp.Interfaces;
using QuizzApp.Models;

namespace QuizzApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // Persist + push to a single user's SignalR group
        public async Task SendToUserAsync(int userId, string message, string type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hub.Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new NotificationDTO
                {
                    Id = notification.Id,
                    Message = message,
                    Type = type,
                    IsRead = false,
                    CreatedAt = notification.CreatedAt
                });
        }

        // Persist one row per QuizTaker + push to each user's group
        public async Task SendToAllTakersAsync(string message, string type)
        {
            var takerIds = await _context.Users
                .Where(u => u.Role == "QuizTaker")
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in takerIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Type = type
                };
                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();

            // Push real-time to each taker
            foreach (var userId in takerIds)
            {
                await _hub.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", new NotificationDTO
                    {
                        Message = message,
                        Type = type,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
            }
        }

        public async Task<IEnumerable<NotificationDTO>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new NotificationDTO
                {
                    Id = n.Id,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task MarkReadAsync(int notificationId, int userId)
        {
            var n = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (n != null)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            unread.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();
        }
    }
}

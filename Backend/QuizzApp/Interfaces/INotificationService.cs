using QuizzApp.DTOs;

namespace QuizzApp.Interfaces
{
    public interface INotificationService
    {
        // Send to a specific user and persist to DB
        Task SendToUserAsync(int userId, string message, string type);

        // Send to all QuizTakers and persist to DB
        Task SendToAllTakersAsync(string message, string type);

        // REST: get notifications for a user
        Task<IEnumerable<NotificationDTO>> GetUserNotificationsAsync(int userId);

        // REST: mark one as read
        Task MarkReadAsync(int notificationId, int userId);

        // REST: mark all as read
        Task MarkAllReadAsync(int userId);
    }
}

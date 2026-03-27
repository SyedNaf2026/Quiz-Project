using Microsoft.AspNetCore.SignalR;

namespace QuizzApp.Hubs
{
    // SignalR hub — clients connect here to receive real-time notifications
    // Each authenticated user joins a group named after their userId
    public class NotificationHub : Hub
    {
        // Called by the Angular client after connecting
        // userId is passed as a query param from the frontend
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
}

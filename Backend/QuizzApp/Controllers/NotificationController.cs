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
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }

        // GET api/notification — returns all notifications for the logged-in user
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(GetUserId());
            return Ok(ApiResponse<IEnumerable<NotificationDTO>>.Ok(notifications));
        }

        // PUT api/notification/{id}/read — mark one notification as read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notificationService.MarkReadAsync(id, GetUserId());
            return Ok(ApiResponse<string>.Ok("Marked as read"));
        }

        // PUT api/notification/read-all — mark all notifications as read
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(GetUserId());
            return Ok(ApiResponse<string>.Ok("All marked as read"));
        }
    }
}

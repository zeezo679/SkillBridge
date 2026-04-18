using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Shared.Common.Params.Notification;
using System.Security.Claims;

namespace SoftBridge.Web.Controllers.Notification
{
    [Authorize]
    public class NotificationsController(INotificationService _notificationService) : AppBaseController
    {
        // get the user id from the claims
        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // 1. get all notifications for the current user with optional query parameters
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationQueryParams queryParams)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUserNotificationsAsync(userId, queryParams);

            return Success(result, "Notifications retrieved successfully");
        }

        // 2. read a specific notification by id
        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);

            return Success("Notification marked as read successfully");
        }

        // 3. read all notifications
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);

            return Success("All notifications marked as read successfully");
        }
    }
}
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Abstraction.IServicesContract.Notification
{
    // This interface defines the contract for the notification service
    // which will be responsible for sending notifications to users based on different strategies 
    // (e.g., email, SMS, push notifications).
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationContentDto message, params NotificationType[] types);

        // --- User/Client Operations (The Bell Icon) ---
        Task<PaginationResponse<NotificationDto>> GetUserNotificationsAsync(string userId, NotificationQueryParams queryParams);
        Task<bool> MarkAsReadAsync(Guid notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);

    }
}

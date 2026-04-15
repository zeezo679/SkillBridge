using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Shared.Common.Pagination;
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
        Task SendNotificationAsync(NotificationContentDto message, NotificationType type);

        // --- User/Client Operations (The Bell Icon) ---
        Task<PaginationResponse<NotificationContentDto>> GetUserNotificationsAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);

    }
}

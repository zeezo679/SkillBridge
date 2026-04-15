using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Shared.Common.Dto.Chat;
using SoftBridge.Shared.Common.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Services.NotificationImplementation
{
    public class NotificationService(IEnumerable<INotificationStrategy> _notificationStrategies) : INotificationService
    {
        public Task<PaginationResponse<NotificationContentDto>> GetUserNotificationsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task SendNotificationAsync(NotificationContentDto message, NotificationType type)
        {
            var strategy = _notificationStrategies.FirstOrDefault(s => s.Type == type);

            await strategy!.DeliverAsync(message);
        }
    }
}

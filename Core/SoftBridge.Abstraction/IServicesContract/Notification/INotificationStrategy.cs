using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Shared.Common.Dto.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Abstraction.IServicesContract.Notification
{
    // we will use the strategy design pattern to implement different notification strategies (e.g., email, SMS, push notifications)
    public interface INotificationStrategy
    {
        NotificationType Type { get; }
        Task DeliverAsync(NotificationContentDto ContentDto);
    }
}

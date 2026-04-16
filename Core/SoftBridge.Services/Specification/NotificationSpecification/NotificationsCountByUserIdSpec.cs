using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.Shared;
using SoftBridge.Shared.Common.Params.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.NotificationSpecification
{
    public class NotificationsCountByUserIdSpec : BaseSpecifications<Notification, Guid>
    {
        public NotificationsCountByUserIdSpec(string userId, NotificationQueryParams queryParams)
            : base(n =>
                (n.UserId == userId) &&
                (!queryParams.IsRead.HasValue || n.IsRead == queryParams.IsRead.Value) 
            )
        {
        }
    }
}

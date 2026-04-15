using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.Shared;
using SoftBridge.Shared.Common.Params;
using SoftBridge.Shared.Common.Params.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.NotificationSpecification
{
    public class NotificationsByUserIdSpec : BaseSpecifications<Notification,Guid>
    {
        public NotificationsByUserIdSpec(string userId , NotificationQueryParams queryParams)
            : base(n =>
                (n.UserId == userId) &&
                (!queryParams.IsRead.HasValue || n.IsRead == queryParams.IsRead.Value)
            )
        {
            ApplyPagenation(queryParams.PageSize ,queryParams.PageIndex);
            AddOrderBy(n => n.CreatedAt, isDescending: true);
        }
    }
}

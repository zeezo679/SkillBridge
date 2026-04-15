using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.NotificationSpecification
{
    public class UnreadNotificationsSpec : BaseSpecifications<Notification, Guid>
    {
        public UnreadNotificationsSpec(string userId)
            : base(n => n.UserId == userId && !n.IsRead)
        {
        }
    }
}

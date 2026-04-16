using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ReviewSpecifications
{
    // used on the public service detail page
    public class ReviewsByServiceIdSpec : BaseSpecifications<Review, Guid>
    {
        public ReviewsByServiceIdSpec(Guid serviceId)
            : base(r => r.ServiceId == serviceId && !r.IsDeleted)
        {
            AddInclude(x => x.Client);
            var clientUser = $"{nameof(Review.Client)}.{nameof(Client.User)}";
            IncludeStrings.Add(clientUser);

            AddOrderBy(r => r.CreatedAt, isDescending: true);
        }
    }
}

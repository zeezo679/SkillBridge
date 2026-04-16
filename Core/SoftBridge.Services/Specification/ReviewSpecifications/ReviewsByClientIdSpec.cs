using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ReviewSpecifications
{
    public class ReviewsByClientIdSpec: BaseSpecifications<Review, Guid>
    {
        public ReviewsByClientIdSpec(Guid clientId)
            :base(r => r.ClientId == clientId && !r.IsDeleted)
        {
            AddInclude(r => r.Provider);
            var providerUser = $"{nameof(Review.Provider)}.{nameof(SProvider.User)}";
            IncludeStrings.Add(providerUser);

            AddInclude(r => r.ServiceRequest);
            var requestService = $"{nameof(Review.ServiceRequest)}.{nameof(ServiceRequest.Service)}";
            IncludeStrings.Add(requestService);

            AddOrderBy(r => r.CreatedAt, isDescending: true);
        }
    }
}

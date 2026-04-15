using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ReviewSpecifications
{
    public class ReviewsByProviderIdSpec: BaseSpecifications<Review, Guid>
    {
        public ReviewsByProviderIdSpec(Guid providerId)
            :base(r => r.ProviderId == providerId && !r.IsDeleted)
        {
            AddInclude(x => x.Client);
            var clientUser = $"{nameof(Review.Client)}.{nameof(Client.User)}";
            IncludeStrings.Add(clientUser);

            AddInclude(r => r.ServiceRequest);
            var requestService = $"{nameof(Review.ServiceRequest)}.{nameof(ServiceRequest.Service)}";
            IncludeStrings.Add(requestService);

            AddOrderBy(r => r.CreatedAt, isDescending: true);
        }
    }
}

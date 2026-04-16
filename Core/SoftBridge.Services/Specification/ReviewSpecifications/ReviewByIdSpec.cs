using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ReviewSpecifications
{
    public class ReviewByIdSpec: BaseSpecifications<Review, Guid>
    {
        public ReviewByIdSpec(Guid reviewId)
            :base(r => r.Id == reviewId && !r.IsDeleted)
        {
            AddInclude(r => r.Client);
            var clientUser = $"{nameof(Review.Client)}.{nameof(Client.User)}";
            IncludeStrings.Add(clientUser);

            AddInclude(r => r.Provider);
            var providerUser = $"{nameof(Review.Provider)}.{nameof(SProvider.User)}";
            IncludeStrings.Add(providerUser);

            AddInclude(r => r.ServiceRequest);
            var requestService = $"{nameof(Review.ServiceRequest)}.{nameof(ServiceRequest.Service)}";
            IncludeStrings.Add(requestService);
        }
    }
}

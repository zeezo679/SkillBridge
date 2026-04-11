using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification
{
    public class ReceivedReviewsSpec: BaseSpecifications<Review, Guid>
    {
        public ReceivedReviewsSpec(Guid providerId)
            :base(r => r.ProviderId == providerId && !r.IsDeleted)
        {
            // Review With Sobih
            var UserInclude = $"{nameof(Review.Client)}.{nameof(Domain.Models.User)}";
            IncludeStrings.Add(UserInclude);

            // Review With Sobih
            var ServiceInclude = $"{nameof(Review.ServiceRequest)}.{nameof(ServiceRequest.Service)}";
            IncludeStrings.Add(ServiceInclude);

            AddOrderBy(r => r.CreatedAt, isDescending: true);

        }
    }
}

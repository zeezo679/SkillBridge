using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ReviewSpecifications
{
    public class RequestForReviewSpec : BaseSpecifications<ServiceRequest, Guid>
    {
        public RequestForReviewSpec(Guid requestId, Guid clientId)
            : base(r => r.Id == requestId
                     && r.ClientId == clientId
                     && !r.IsDeleted)
        {
            AddInclude(r => r.Service);
            AddInclude(r => r.Provider);

            AddInclude(r => r.Provider);
            var providerUser = $"{nameof(Review.Provider)}.{nameof(SProvider.User)}";
            IncludeStrings.Add(providerUser);

            AddInclude(x => x.Review);   // check if review already exists
        }
    }
}

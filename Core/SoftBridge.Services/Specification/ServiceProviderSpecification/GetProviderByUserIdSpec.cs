using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification
{
    public class GetProviderByUserIdSpec: BaseSpecifications<SProvider, Guid>
    {
        public GetProviderByUserIdSpec(string userId)
            :base(p => p.UserId == userId && !p.IsDeleted)
        {
            AddInclude(p => p.User);
        }
    }
}

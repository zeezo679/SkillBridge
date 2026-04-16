using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ProviderSpecifications
{
    public class ProviderByIdWithUserSpec : BaseSpecifications<SProvider,Guid>
    {
        public ProviderByIdWithUserSpec(Guid providerId)
            : base(p => p.Id == providerId)
        {
            AddInclude(p => p.User);
        }
    }
}

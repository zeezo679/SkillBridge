using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;

namespace SoftBridge.Services.Specification.ProviderSpecifications;

public class ProviderByUserIdWithUserSpec : BaseSpecifications<SProvider,Guid>
{
    public ProviderByUserIdWithUserSpec(string userId)
        : base(p => p.UserId == userId)
    {
        AddInclude(p => p.User);
    }
    
}


using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServicesSpecifications
{
    public class ProviderProfileByAppUserIdSpec : BaseSpecifications<SProvider, Guid>
    {
        public ProviderProfileByAppUserIdSpec(string applicationUserId)
            : base(p => p.UserId == applicationUserId)
        {
            AddInclude(p => p.User);
        }
    }
}

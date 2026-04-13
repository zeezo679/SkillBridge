using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification.ToVerifyDeleteAccount
{
    // all non-deleted services — to soft-delete on account removal
    public class ActiveServicesByProviderSpec: BaseSpecifications<Service, Guid>
    {
        public ActiveServicesByProviderSpec(Guid providerId)
            :base(s => s.ProviderId == providerId
                    && !s.IsDeleted)
        {
            AddInclude(s => s.Images); // needed to delete image files from disk
        }
    }
}

using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification.ToVerifyDeleteAccount
{
    // checks for requests the provider is still obligated to fulfill
    public class ActiveRequestsByProviderSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public ActiveRequestsByProviderSpec(Guid providerId)
            :base(s => s.ProviderId == providerId
                    && !s.IsDeleted
                    && s.Status == RequestStatus.Accepted)
        {
            
        }
    }
}

using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification.ToVerifyDeleteAccount
{
    public class PendingRequestsByProviderSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        // requests that can be safely cancelled on account deletion
        public PendingRequestsByProviderSpec(Guid providerId)
            :base(s => s.ProviderId == providerId
                    && !s.IsDeleted
                    && (s.Status == RequestStatus.Pending
                    || s.Status == RequestStatus.Completed))
        {
            
        }
    }
}

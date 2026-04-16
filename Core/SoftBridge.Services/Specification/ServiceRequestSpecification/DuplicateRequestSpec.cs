using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceRequestSpecification
{
    public class DuplicateRequestSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public DuplicateRequestSpec(Guid serviceId, Guid clientId)
            :base(r => r.ServiceId == serviceId
                    && r.ClientId == clientId
                    && !r.IsDeleted
                    && (r.Status == RequestStatus.Pending
                    || r.Status == RequestStatus.Accepted))
        {
            
        }
    }
}
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceRequestSpecification
{
    // validates ownership before Accept / Reject / Complete
    public class RequestOwnershipSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public RequestOwnershipSpec(Guid requestId)
        : base(r => r.Id == requestId && !r.IsDeleted)
        {
        }
    }
}

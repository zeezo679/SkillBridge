using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServicesSpecifications
{
    public class ServiceForDeletionSpec : BaseSpecifications<Service, Guid>
    {
        public ServiceForDeletionSpec(Guid serviceId)
            : base(s => s.Id == serviceId)
        {
            Includes.Add(s => s.ServiceRequests);
            Includes.Add(s => s.Images);
        }
    }
}

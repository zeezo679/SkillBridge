using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification
{
    public class GetProviderServiceByIdSpec: BaseSpecifications<Service, Guid>
    {
        public GetProviderServiceByIdSpec(Guid serviceId, Guid providerId)
            :base(s => s.Id == serviceId 
                    && s.ProviderId == providerId
                    && !s.IsDeleted)
        {
            AddInclude(s => s.Category);
            AddInclude(s => s.Images);
        }
    }
}

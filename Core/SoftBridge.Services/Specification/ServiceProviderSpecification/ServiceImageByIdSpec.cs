using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification
{
    public class ServiceImageByIdSpec: BaseSpecifications<ServiceImage, Guid>
    {
        public ServiceImageByIdSpec(Guid imageId, Guid serviceId)
            :base(i => i.Id == imageId 
                    && i.ServiceId == serviceId
                    && !i.IsDeleted)
        {        
        }
    }
}

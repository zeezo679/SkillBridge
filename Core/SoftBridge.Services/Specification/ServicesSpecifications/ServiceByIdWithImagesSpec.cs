using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServicesSpecifications
{
    public class ServiceByIdWithImagesSpec : BaseSpecifications<Service, Guid>
    {
        public ServiceByIdWithImagesSpec(Guid serviceId)
            : base(s => s.Id == serviceId)
        {
            AddInclude(s => s.Images);
        }
    }
}

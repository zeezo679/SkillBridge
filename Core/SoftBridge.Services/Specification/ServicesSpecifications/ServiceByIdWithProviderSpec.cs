using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServicesSpecifications
{
    public class ServiceByIdWithProviderSpec : BaseSpecifications<Service, Guid>
    {
        public ServiceByIdWithProviderSpec(Guid serviceId)
            : base(s => s.Id == serviceId)
        {
            Includes.Add(s => s.Provider);
        }
    }
}

using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServicesSpecifications
{
    public class ServiceDetailsByIdSpec : BaseSpecifications<Service, Guid>
    {
        public ServiceDetailsByIdSpec(Guid serviceId)
            : base(s => s.Id == serviceId)
        {
            AddInclude(s => s.Images);
            AddInclude(s => s.Category);
            AddInclude(s => s.Provider);
            AddInclude("Provider.User");
        }
    }
}

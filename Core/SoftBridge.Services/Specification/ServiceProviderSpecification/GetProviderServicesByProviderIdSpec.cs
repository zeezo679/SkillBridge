using MailKit.Search;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification
{
    public class GetProviderServicesByProviderIdSpec: BaseSpecifications<Service, Guid>
    {
        public GetProviderServicesByProviderIdSpec(Guid providerId)
            :base(s => s.ProviderId == providerId && !s.IsDeleted)
        {
            AddInclude(s => s.Category);
            AddInclude(s => s.Images);
            AddOrderBy(s => s.CreatedAt, isDescending: true);
        }
    }
}

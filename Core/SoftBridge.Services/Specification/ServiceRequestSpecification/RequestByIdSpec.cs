using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceRequestSpecification
{
    public class RequestByIdSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        // full detail load — used for GetRequestDetailsAsync
        public RequestByIdSpec(Guid requestId)
            :base(r => r.Id == requestId && !r.IsDeleted)
        {
            AddInclude(r => r.Service);
            var serviceCategory = $"{nameof(ServiceRequest.Service)}.{nameof(Service.Category)}";
            IncludeStrings.Add(serviceCategory);

            AddInclude(r => r.Provider);
            var providerUser = $"{nameof(ServiceRequest.Provider)}.{nameof(SProvider.User)}";
            IncludeStrings.Add(providerUser);

            AddInclude(r => r.Client);
            var clientUser = $"{nameof(ServiceRequest.Client)}.{nameof(Client.User)}";
            IncludeStrings.Add(clientUser);

            AddInclude(r => r.Review);
        }
    }
}

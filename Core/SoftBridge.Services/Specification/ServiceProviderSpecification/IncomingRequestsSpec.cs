using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceProviderSpecification
{
    public class IncomingRequestsSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public IncomingRequestsSpec(Guid providerId, RequestStatus? status = null)
            :base(r => r.ProviderId == providerId
                    && (status == null || r.Status == status)
                    && !r.IsDeleted)
        {
            AddInclude(r => r.Service);

            var UserInclude = $"{nameof(ServiceRequest.Client)}.{nameof(Domain.Models.User)}";
            IncludeStrings.Add(UserInclude);

            AddOrderBy(s => s.CreatedAt, isDescending: true);
        }
    }
}

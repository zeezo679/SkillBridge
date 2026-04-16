using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Params.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceRequestSpecification
{
    public class ClientRequestsSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public ClientRequestsSpec(Guid clientId, RequestQueryParams qp)
            :base(r => r.ClientId == clientId
                    && !r.IsDeleted
                    && (qp.Status == null || r.Status == qp.Status)
                    && (qp.FromDate == null || r.CreatedAt >= qp.FromDate)
                    && (qp.ToDate == null || r.CreatedAt <= qp.ToDate)
                    && (qp.Search == null || r.Service.Title.ToLower().Contains(qp.Search)))
        {
            AddInclude(r => r.Service);

            AddInclude(r => r.Provider);
            var providerUser = $"{nameof(ServiceRequest.Provider)}.{nameof(SProvider.User)}";
            IncludeStrings.Add(providerUser);

            AddInclude(r => r.Client);
            var clientUser = $"{nameof(ServiceRequest.Client)}.{nameof(Client.User)}";
            IncludeStrings.Add(clientUser);

            AddOrderBy(r => r.CreatedAt, isDescending: true);

            ApplyPagenation(qp.PageSize, qp.PageIndex);

        }
    }
}

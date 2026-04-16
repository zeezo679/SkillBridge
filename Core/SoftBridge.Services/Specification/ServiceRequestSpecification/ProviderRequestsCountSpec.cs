using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Params.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceRequestSpecification
{
    public class ProviderRequestsCountSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public ProviderRequestsCountSpec(Guid providerId, RequestQueryParams qp)
        : base(r => r.ProviderId == providerId
                 && !r.IsDeleted
                 && (qp.Status == null || r.Status == qp.Status)
                 && (qp.FromDate == null || r.CreatedAt >= qp.FromDate)
                 && (qp.ToDate == null || r.CreatedAt <= qp.ToDate)
                 && (qp.Search == null
                  || r.Service.Title.ToLower().Contains(qp.Search)
                  || r.Client.User.FullName.ToLower().Contains(qp.Search)))
        {
        }
    }
}

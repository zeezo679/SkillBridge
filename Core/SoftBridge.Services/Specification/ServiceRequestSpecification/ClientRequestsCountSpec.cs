using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Params.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceRequestSpecification
{
    // used exclusively for GetCountAsync to get the total for pagination data
    public class ClientRequestsCountSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public ClientRequestsCountSpec(Guid clientId, RequestQueryParams qp)
        : base(r => r.ClientId == clientId
                 && !r.IsDeleted
                 && (qp.Status == null || r.Status == qp.Status)
                 && (qp.FromDate == null || r.CreatedAt >= qp.FromDate)
                 && (qp.ToDate == null || r.CreatedAt <= qp.ToDate)
                 && (qp.Search == null
                  || r.Service.Title.ToLower().Contains(qp.Search)
                  || r.Provider.User.FullName.ToLower().Contains(qp.Search)))
        {
        }
    }
}

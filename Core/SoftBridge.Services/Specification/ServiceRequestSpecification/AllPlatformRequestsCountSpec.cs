using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Params.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServiceRequestSpecification
{
    public class AllPlatformRequestsCountSpec: BaseSpecifications<ServiceRequest, Guid>
    {
        public AllPlatformRequestsCountSpec(RequestQueryParams qp)
        : base(r => !r.IsDeleted
                 && (qp.Status == null || r.Status == qp.Status)
                 && (qp.FromDate == null || r.CreatedAt >= qp.FromDate)
                 && (qp.ToDate == null || r.CreatedAt <= qp.ToDate)
                 && (qp.Search == null
                  || r.Service.Title.ToLower().Contains(qp.Search)
                  || r.Client.User.FullName.ToLower().Contains(qp.Search)
                  || r.Provider.User.FullName.ToLower().Contains(qp.Search)))
        {
        }
    }
}

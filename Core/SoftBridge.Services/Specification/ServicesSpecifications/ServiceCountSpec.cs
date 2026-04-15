using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Shared.Common.Params.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Specification.ServicesSpecifications
{
    public class ServiceCountSpec : BaseSpecifications<Service, Guid>
    {
        public ServiceCountSpec(ServiceQueryParams queryParams)
            : base(s =>
                (string.IsNullOrEmpty(queryParams.Search) ||
                 s.Title.ToLower().Contains(queryParams.Search) ||
                 s.Description.ToLower().Contains(queryParams.Search)) &&

                (!queryParams.CategoryId.HasValue || s.CategoryId == queryParams.CategoryId) &&

                (!queryParams.MinPrice.HasValue || s.Price >= queryParams.MinPrice) &&
                (!queryParams.MaxPrice.HasValue || s.Price <= queryParams.MaxPrice) &&

                (!queryParams.Status.HasValue || s.Status == queryParams.Status)
            )
        {
        }
    }
}

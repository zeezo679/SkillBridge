using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Shared.Common.Params.Service;

namespace SoftBridge.Services.Specification.ServicesSpecifications
{
    public class ServiceWithFiltersForProviderSpec : BaseSpecifications<Service,Guid>
    {
        public ServiceWithFiltersForProviderSpec(Guid providerId, ServiceQueryParams queryParams)
        : base(s =>
            (s.ProviderId == providerId) &&

            (string.IsNullOrEmpty(queryParams.Search) ||
             s.Title.ToLower().Contains(queryParams.Search) ||
             s.Description.ToLower().Contains(queryParams.Search)) &&

            (!queryParams.CategoryId.HasValue || s.CategoryId == queryParams.CategoryId) &&

            (!queryParams.MinPrice.HasValue || s.Price >= queryParams.MinPrice) &&
            (!queryParams.MaxPrice.HasValue || s.Price <= queryParams.MaxPrice) &&
            (!queryParams.Status.HasValue || s.Status == queryParams.Status)
        )
        {
            Includes.Add(s => s.Images);
            Includes.Add(s => s.Category);

            ApplyPagenation(queryParams.PageSize, queryParams.PageIndex);

            AddOrderBy(s => s.CreatedAt, true);
        }
    }
}

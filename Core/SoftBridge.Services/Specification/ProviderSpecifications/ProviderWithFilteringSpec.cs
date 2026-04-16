using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Shared.Common.Params.Admin;

namespace SoftBridge.Services.Specification.ProviderSpecifications;

public class ProviderWithFilteringSpec : BaseSpecifications<SProvider, Guid>
{
    public ProviderWithFilteringSpec(ProviderQueryParams queryParams) : base(provider =>
        (queryParams.Search == null ||
         provider.User.FullName.Contains(queryParams.Search) ||
         provider.User.Email != null && provider.User.Email.Contains(queryParams.Search)) &&
        (!queryParams.Status.HasValue || provider.Status == queryParams.Status) &&
        (!queryParams.MinRating.HasValue || provider.AverageRating >= queryParams.MinRating) &&
        (!queryParams.RegisteredFrom.HasValue || provider.CreatedAt >= queryParams.RegisteredFrom) &&
        (!queryParams.RegisteredTo.HasValue || provider.CreatedAt <= queryParams.RegisteredTo))
    {
        AddInclude(provider => provider.User);
        AddOrderBy(provider => provider.CreatedAt, true); // Order by newest providers first
    }
}

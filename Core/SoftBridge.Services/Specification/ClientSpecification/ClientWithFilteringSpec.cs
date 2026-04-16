using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Shared.Common.Params.Admin;

namespace SoftBridge.Services.Specification.ClientSpecification;

public class ClientWithFilteringSpec : BaseSpecifications<Client, Guid>
{
    public ClientWithFilteringSpec(ClientQueryParams queryParams) : base(client =>
    (queryParams.Search == null ||
     client.User.FullName.Contains(queryParams.Search) ||
     client.User.Email != null && client.User.Email.Contains(queryParams.Search))
    )
    {
        AddInclude(client => client.User);
        AddOrderBy(client => client.CreatedAt, true); // Order by newest clients first
    }

}

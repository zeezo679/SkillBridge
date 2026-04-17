using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Shared.Common.Params.Admin;

namespace SoftBridge.Services.Specification.ClientSpecification;

public class ClientCountSpec : BaseSpecifications<Client, Guid>
{
    public ClientCountSpec(ClientQueryParams queryParams) : base(client =>
        (queryParams.Search == null ||
         client.User.FullName.Contains(queryParams.Search) ||
         client.User.Email != null && client.User.Email.Contains(queryParams.Search))
        )
        {
    
        }
}

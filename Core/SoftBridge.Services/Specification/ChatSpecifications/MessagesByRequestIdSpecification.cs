using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.Shared;
using SoftBridge.Shared.Common.Params;

namespace SoftBridge.Services.Specification.ChatSpecifications;

public class MessagesByRequestIdSpecification : BaseSpecifications<Message, Guid>
{
    public MessagesByRequestIdSpecification(Guid requestId, BaseQueryParams queryParams) : base(m => m.RequestId == requestId)
    {
        AddInclude(m => m.Sender);

        ApplyPagenation(queryParams.PageSize, queryParams.PageIndex);
        AddOrderBy(m => m.SentAt);
    }

}

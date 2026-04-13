using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.Shared;

namespace SoftBridge.Services.Specification.ChatSpecifications;

public class MessagesCountByRequestIdSpecification : BaseSpecifications<Message, Guid>
{
    public MessagesCountByRequestIdSpecification(Guid requestId) : base(m => m.RequestId == requestId)
    {
    }

}

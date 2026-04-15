using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.Shared;

namespace SoftBridge.Services.Specification.ChatSpecifications;

public class UnreadMessagesSpec : BaseSpecifications<Message, Guid>
{
    public UnreadMessagesSpec(Guid requestId, string receiverId) : base (   
        m => m.RequestId == requestId 
        && m.ReceiverId == receiverId 
        && !m.IsRead)
    {
    }
}

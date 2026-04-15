using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;

namespace SoftBridge.Services.Specification.ChatSpecifications;

public class ChatInboxSpec : BaseSpecifications<ServiceRequest, Guid>
{
    public ChatInboxSpec(string userId) : base(s => 
        (s.Provider.UserId == userId || s.Client.UserId == userId) 
        && s.Status == RequestStatus.Accepted) 
    {
        AddInclude(s => s.Client);
        AddInclude(s => s.Provider);
        AddInclude(s => s.Messages);
        AddInclude(s => s.Service);
    }
}

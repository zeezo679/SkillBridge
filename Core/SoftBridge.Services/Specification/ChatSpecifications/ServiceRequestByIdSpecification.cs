using System;
using SoftBridge.Domain.Contracts.Specifications.BaseSpec;
using SoftBridge.Domain.Models.OrderAggregates;

namespace SoftBridge.Services.Specification.ChatSpecifications;

public class ServiceRequestByIdSpecification : BaseSpecifications<ServiceRequest, Guid>
{
    public ServiceRequestByIdSpecification(Guid requestId) : base(r => r.Id == requestId)
    {
        AddInclude(r => r.Client);
        AddInclude(r => r.Provider);
    }
}
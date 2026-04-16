using System;
using AutoMapper;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Services.Specification.ClientSpecification;
using SoftBridge.Services.Specification.ProviderSpecifications;
using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Admin;

namespace SoftBridge.Services.Services.Admin;

public class AdminService(IUnitOfWork unitOfWork, IMapper mapper) : IAdminService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    public async Task ApproveProviderAsync(string userId, string adminId)
    {
        var provider = await GetValidatedProviderHelper(userId);

        provider.Status = ProviderAccountStatus.Approved;
        provider.ApprovedAt = DateTime.UtcNow;
        provider.ApproveByAdminId = adminId;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginationResponse<ClientProfileDto>> GetAllClientsAsync(ClientQueryParams queryParams)
    {
        var clientRepo = _unitOfWork.GetRepository<Client, Guid>();

        var spec = new ClientWithFilteringSpec(queryParams);
        var countSpec = new ClientCountSpec(queryParams);

        var clients = await clientRepo.GetAllWithSpecAsync(spec);
        var totalCount = await clientRepo.GetCountAsync(countSpec);

        var mappedClients = _mapper.Map<IReadOnlyList<ClientProfileDto>>(clients);
        return new PaginationResponse<ClientProfileDto>(queryParams.PageIndex, queryParams.PageSize, totalCount, mappedClients);
    }

    public async Task<PaginationResponse<ProviderProfileDto>> GetAllProvidersAsync(ProviderQueryParams queryParams)
    {
        var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();
        
        var spec = new ProviderWithFilteringSpec(queryParams);
        var countSpec = new ProviderCountSpec(queryParams);

        var providers = await providerRepo.GetAllWithSpecAsync(spec);
        var totalCount = await providerRepo.GetCountAsync(countSpec);

        var mappedProviders = _mapper.Map<IReadOnlyList<ProviderProfileDto>>(providers);
        return new PaginationResponse<ProviderProfileDto>(queryParams.PageIndex, queryParams.PageSize, totalCount, mappedProviders);
    }

    public async Task RejectProviderAsync(string userId, string adminId)
    {
        var provider = await GetValidatedProviderHelper(userId);

        provider.Status = ProviderAccountStatus.Rejected;
        provider.ApprovedAt = null;
        provider.ApproveByAdminId = null;
        await _unitOfWork.SaveChangesAsync();
    }


    // Additional private helper methods for filtering and pagination can be added here
    private async Task<SProvider> GetValidatedProviderHelper(string userId)
    {
        var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();
        var providerSpec = new ProviderByUserIdWithUserSpec(userId);
        var provider = await providerRepo.GetByIdWithSpecAsync(providerSpec);

        if (provider == null)
            throw new ProviderNotFoundException($"Provider with ID {userId} not found.");

        return provider;
    }
}

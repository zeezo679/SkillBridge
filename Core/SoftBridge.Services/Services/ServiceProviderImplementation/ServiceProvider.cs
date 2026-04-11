using AutoMapper;
using SoftBridge.Abstraction.IServices.Attachement;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Services.Specification.ServiceProviderSpecification;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Services.ServiceProviderImplementation
{
    public class ServiceProvider : IProviderProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;
        public ServiceProvider(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
        }

        // private helper
        // resolves Provider from userId — reused in every method, To verify the identity of the provider
        private async Task<SProvider> GetProviderOrThrowAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<SProvider, Guid>();
            var spec = new GetProviderByUserIdSpec(userId);
            var provider = await repo.GetByIdWithSpecAsync(spec);

            if (provider == null)
                throw new ProviderNotFoundException($"No provider profile found for user {userId}");

            if (provider.Status != ProviderAccountStatus.Approved)
                throw new ProviderUnauthorizedException();

            return provider;
        }
        public Task<ProviderProfileDto> GetMyProfileAsync(string userId)
        {
            throw new NotImplementedException();
        }
        public Task<IncomingRequestDto> AcceptRequestAsync(string userId, Guid requestId, RespondToRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceWithProviderDto> AddServiceAsync(string userId, AddServiceWithProviderDto dto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAccountAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteServiceAsync(string userId, Guid serviceId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IncomingRequestDto>> GetIncomingRequestsAsync(string userId, RequestStatus? status)
        {
            throw new NotImplementedException();
        }


        public Task<IReadOnlyList<ReceivedReviewDto>> GetMyReviewsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ServiceWithProviderDto>> GetMyServicesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IncomingRequestDto> RejectRequestAsync(string userId, Guid requestId, RespondToRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<IncomingRequestDto> RespondToRequestAsync(string userId, Guid requestId, RespondToRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ProviderProfileDto> UpdateProfileAsync(string userId, UpdateProviderProfileDto updateDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceWithProviderDto> UpdateServiceAsync(string userId, Guid serviceId, UpdateServiceWithProviderDto dto)
        {
            throw new NotImplementedException();
        }
    }
}

using AutoMapper;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Request;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Exceptions.BadRequestModels;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Services.Services.ClientImplementation;
using SoftBridge.Services.Specification.ServiceRequestSpecification;
using SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Services.RequestImplementation
{
    public class RequestService : IRequestWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClientProfileService _clientService;
        private readonly IProviderProfileService _providerService;

        public RequestService(IUnitOfWork unitOfWork, IMapper mapper,
                IClientProfileService clientService, IProviderProfileService providerService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clientService = clientService;
            _providerService = providerService;
        }

        // Client Operations
        public async Task<RequestDto> SendServiceRequestAsync(CreateRequestDto createRequestDto, Guid clientId)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();

            // Load Service - must be approved to recieve requests
            var service = await serviceRepo.GetByIdAsync(createRequestDto.ServiceId);

            if (service == null || service.IsDeleted)
                throw new RequestNotFoundException("Service not found.");

            if (service.Status != ServiceStatus.Approved)
                throw new RequestBadRequestException("This service is not currently available.");

            // Block duplicate active requests from the same client for the same service
            var duplicateSpec = new DuplicateRequestSpec(createRequestDto.ServiceId, clientId);
            var duplicateCount = await requestRepo.GetCountAsync(duplicateSpec);

            if(duplicateCount > 0)
                throw new RequestBadRequestException("You already have an active request for this service.");

            var request = new ServiceRequest
            {
                Id = Guid.NewGuid(),
                ServiceId = createRequestDto.ServiceId,
                ClientId = clientId,
                ProviderId = service.ProviderId,
                Message = createRequestDto.Message.Trim(),
                AgreedPrice = createRequestDto.AgreedPrice ?? service.Price,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await requestRepo.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // 4. *** reload with navigations for mapping ***
            var detailSpec = new RequestByIdSpec(request.Id);
            var createdRequest = await requestRepo.GetByIdWithSpecAsync(detailSpec);

            return _mapper.Map<RequestDto>(createdRequest);
        }
        public async Task<PaginationResponse<RequestDto>> GetClientRequestsAsync(Guid clientId, RequestQueryParams queryParams)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            // run both queries in parallel — data page and total count
            var dataSpec = new ClientRequestsSpec(clientId, queryParams);
            var countSpec = new ClientRequestsCountSpec(clientId, queryParams);

            var dataTask = requestRepo.GetAllWithSpecAsync(dataSpec);
            var countTask = requestRepo.GetCountAsync(countSpec);

            await Task.WhenAll(dataTask, countTask);

            var data = _mapper.Map<IReadOnlyList<RequestDto>>(dataTask.Result);
            var count = countTask.Result;

            return new PaginationResponse<RequestDto>(
                queryParams.PageIndex,
                queryParams.PageSize,
                count,
                data);
        }

        // Provider Operations
        public async Task<PaginationResponse<RequestDto>> GetProviderRequestsAsync(Guid providerId, RequestQueryParams queryParams)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            var dataSpec = new ProviderRequestsSpec(providerId, queryParams);
            var countSpec = new ProviderRequestsCountSpec(providerId, queryParams);

            var dataTask = requestRepo.GetAllWithSpecAsync(dataSpec);
            var countTask = requestRepo.GetCountAsync(countSpec);

            await Task.WhenAll(dataTask, countTask);

            var data = _mapper.Map<IReadOnlyList<RequestDto>>(dataTask.Result);
            var count = countTask.Result;

            return new PaginationResponse<RequestDto>(
                queryParams.PageIndex,
                queryParams.PageSize,
                count,
                data);
        }
        public async Task<bool> AcceptRequestAsync(Guid requestId, Guid providerId)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var spec = new RequestOwnershipSpec(requestId);
            var request = await requestRepo.GetByIdWithSpecAsync(spec);

            if(request == null)
                throw new RequestNotFoundException("Request not found.");

            // ownership check — provider can only act on their own requests
            if (request.ProviderId != providerId)
                throw new RequestUnauthorizedException();

            if (request.Status != RequestStatus.Pending)
                throw new RequestBadRequestException("Only pending requests can be accepted.");

            request.Status = RequestStatus.Accepted;
            request.AcceptedAt = DateTime.UtcNow;

            requestRepo.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<bool> RejectRequestAsync(Guid requestId, Guid providerId, string rejectionReason)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var spec = new RequestOwnershipSpec(requestId);
            var request = await requestRepo.GetByIdWithSpecAsync(spec);

            if (request == null)
                throw new RequestNotFoundException("Request not found.");

            if(request.ProviderId != providerId)
                throw new RequestUnauthorizedException();

            if(request.Status != RequestStatus.Pending)
                throw new RequestBadRequestException("Only pending requests can be rejected.");

            if (string.IsNullOrWhiteSpace(rejectionReason))
                throw new BadRequestExceptionCustome(
                    "A rejection reason must be provided.");

            request.Status = RequestStatus.Rejected;
            request.RejectionReason = rejectionReason.Trim();

            requestRepo.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // Common Operations
        public async Task<bool> CompleteRequestAsync(Guid requestId, Guid userId)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var spec = new RequestOwnershipSpec(requestId);
            var request = await requestRepo.GetByIdWithSpecAsync(spec);

            if(request == null)
                throw new RequestNotFoundException("Request not found.");

            if (request.ProviderId != userId)
                throw new RequestUnauthorizedException();

            if(request.Status != RequestStatus.Accepted)
                throw new RequestBadRequestException("Only accepted requests can be completed.");

            request.Status = RequestStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;

            requestRepo.Update(request);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<RequestDetailsDto> GetRequestDetailsAsync(Guid requestId)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var spec = new RequestByIdSpec(requestId);
            var request = await requestRepo.GetByIdWithSpecAsync(spec);

            if (request == null)
                throw new RequestNotFoundException("Request not found.");

            return _mapper.Map<RequestDetailsDto>(request);
        }

        // Admin Operations
        public async Task<PaginationResponse<RequestDto>> GetAllPlatformRequestsAsync(RequestQueryParams queryParams)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            var dataSpec = new AllPlatformRequestsSpec(queryParams);
            var countSpec = new AllPlatformRequestsCountSpec(queryParams);

            var dataTask = requestRepo.GetAllWithSpecAsync(dataSpec);
            var countTask = requestRepo.GetCountAsync(countSpec);

            await Task.WhenAll(dataTask, countTask);

            var data = _mapper.Map<IReadOnlyList<RequestDto>>(dataTask.Result);
            var count = countTask.Result;

            return new PaginationResponse<RequestDto>(
                queryParams.PageIndex,
                queryParams.PageSize,
                count,
                data);
        }
    }
}

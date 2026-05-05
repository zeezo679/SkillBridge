using AutoMapper;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Abstraction.IServicesContract.Request;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Exceptions.BadRequestModels;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Services.Services.ClientImplementation;
using SoftBridge.Services.Services.NotificationImplementation;
using SoftBridge.Services.Specification.ServiceRequestSpecification;
using SoftBridge.Shared.Common.Dto.Notification;
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
        private readonly INotificationService _notificationService;

        public RequestService(IUnitOfWork unitOfWork, IMapper mapper,
                IClientProfileService clientService, IProviderProfileService providerService , INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clientService = clientService;
            _providerService = providerService;
            _notificationService = notificationService;
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
            
            var notificationMessage = new NotificationContentDto
            {
                // include Provider & Services
                UserId = createdRequest!.Provider.UserId,
                Email = createdRequest.Provider.User?.Email,
                Subject = "طلب خدمة جديد 🔔",
                Body = $"لقد تلقيت طلب جديد من عميل لخدمة '{createdRequest.Service.Title}'. يرجى مراجعته في أقرب وقت.",
                ReferenceId = createdRequest.Id
            };

            await _notificationService.SendNotificationAsync(
                notificationMessage,
                NotificationType.Push,
                NotificationType.Email
            );

            return _mapper.Map<RequestDto>(createdRequest);
        }
        public async Task<PaginationResponse<RequestDto>> GetClientRequestsAsync(Guid clientId, RequestQueryParams queryParams)
        {
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();

            var dataSpec = new ClientRequestsSpec(clientId, queryParams);
            var countSpec = new ClientRequestsCountSpec(clientId, queryParams);

            var requests = await requestRepo.GetAllWithSpecAsync(dataSpec);
            var count = await requestRepo.GetCountAsync(countSpec);

            var data = _mapper.Map<IReadOnlyList<RequestDto>>(requests);

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

            var requests = await requestRepo.GetAllWithSpecAsync(dataSpec);
            var count = await requestRepo.GetCountAsync(countSpec);

            var data = _mapper.Map<IReadOnlyList<RequestDto>>(requests);

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

            var notificationMessage = new NotificationContentDto
            {
                // RequestOwnershipSpec  Include  Client.User
                UserId = request.Client.UserId,
                Email = request.Client.User?.Email,
                Subject = "تم قبول طلبك! 🎉",
                Body = "وافق مقدم الخدمة على طلبك. يمكنك الآن التواصل معه والبدء في التنفيذ.",
                ReferenceId = request.Id
            };

            await _notificationService.SendNotificationAsync(
                notificationMessage, NotificationType.Push, NotificationType.Email);

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

            var notificationMessage = new NotificationContentDto
            {
                UserId = request.Client.UserId,
                Email = request.Client.User?.Email,
                Subject = "تحديث بخصوص طلبك ⚠️",
                Body = $"نأسف، قام مقدم الخدمة برفض طلبك. السبب: {request.RejectionReason}",
                ReferenceId = request.Id
            };

            await _notificationService.SendNotificationAsync(
                notificationMessage, NotificationType.Push, NotificationType.Email);

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

            var notificationMessage = new NotificationContentDto
            {
                UserId = request.Client.UserId,
                Email = request.Client.User?.Email,
                Subject = "اكتمل طلبك! ✅",
                Body = "قام مقدم الخدمة بإنهاء العمل على طلبك. يرجى مراجعة العمل وتقييم الخدمة.",
                ReferenceId = request.Id
            };

            await _notificationService.SendNotificationAsync(
                notificationMessage, NotificationType.Push, NotificationType.Email);

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

            var requests = await requestRepo.GetAllWithSpecAsync(dataSpec);
            var count = await requestRepo.GetCountAsync(countSpec);

            var data = _mapper.Map<IReadOnlyList<RequestDto>>(requests);

            return new PaginationResponse<RequestDto>(
                queryParams.PageIndex,
                queryParams.PageSize,
                count,
                data);
        }
    }
}

using AutoMapper;
using SoftBridge.Abstraction.IServices.Attachement;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Exceptions.BadRequestModels;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Services.Specification.ServiceProviderSpecification;
using SoftBridge.Services.Specification.ServiceProviderSpecification.ToVerifyDeleteAccount;
using SoftBridge.Shared.Common.Dto.Attachement;
using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Services.ServiceProviderImplementation
{
    public class ServiceProviderService : IProviderProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;
        private readonly INotificationService _notificationService; 

        public ServiceProviderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAttachmentService attachmentService,
            INotificationService notificationService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
            _notificationService = notificationService;
        }

        // private helper
        // resolves Provider from userId, To verify the identity of the provider
        // use in special use cases
        private async Task<SProvider> GetProviderOrThrowAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<SProvider, Guid>();
            var spec = new GetProviderByUserIdSpec(userId);
            var provider = await repo.GetByIdWithSpecAsync(spec);

            if (provider == null)
                throw new ProviderNotFoundException($"No provider profile found for user {userId}");

            //if (provider.Status != ProviderAccountStatus.Approved)
            //    throw new ProviderUnauthorizedException();

            return provider;
        }
        public async Task<ProviderProfileDto> GetMyProfileAsync(string userId)
        {
            var provider = await GetProviderOrThrowAsync(userId);

            return _mapper.Map<ProviderProfileDto>(provider);
        }
        public async Task<ProviderProfileDto> UpdateProfileAsync(string userId, UpdateProviderProfileDto updateDto)
        {
            var provider = await GetProviderOrThrowAsync(userId);
            var repo = _unitOfWork.GetRepository<SProvider, Guid>();

            // handle profile picture
            if (updateDto.ProfilePicture != null)
            {
                var folder = Path.Combine("Providers", userId, "Profile");
                var imagePath = await _attachmentService.UploadFileAsync(new UploadFileDto
                {
                    File = updateDto.ProfilePicture,
                    FolderName = folder
                });

                if(!string.IsNullOrEmpty(provider.ProfileImageUrl))
                    await _attachmentService.DeleteFileAsync(provider.ProfileImageUrl);

                provider.ProfileImageUrl = imagePath;
            }

            // handle CV
            if (updateDto.Cv != null)
            {
                var folder = Path.Combine("Provider", userId, "CV");
                var imagePath = await _attachmentService.UploadFileAsync(new UploadFileDto
                {
                    File = updateDto.Cv,
                    FolderName = folder
                });

                if(!string.IsNullOrEmpty(provider.CvUrl))
                    await _attachmentService.DeleteFileAsync(provider.CvUrl);

                provider.CvUrl = imagePath;

            }

            provider.User.FullName = updateDto.FullName;
            provider.Bio = updateDto.Bio;
            provider.PortfolioLink = updateDto.PortfolioLink;

            repo.Update(provider);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProviderProfileDto>(provider);
        }
        public async Task DeleteAccountAsync(string userId)
        {
            var provider = await GetProviderOrThrowAsync(userId);

            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();
            var repo = _unitOfWork.GetRepository<SProvider, Guid>();

            // Extra if I want to apply this business rule
            //if(provider.Services.Any())
            //    throw new ProviderBadRequestException($"Provider {userId} has active services and cannot be deleted.");

            // 1. block deletion if provider has active obligations
            var activeRequestsSpec = new ActiveRequestsByProviderSpec(provider.Id);
            var activeRequestsCount = await requestRepo.GetCountAsync(activeRequestsSpec);

            if(activeRequestsCount > 0)
                throw new ProviderBadRequestException(
                    $"Cannot delete account. You have {activeRequestsCount} active " +
                    $"request(s) that must be completed or cancelled first.");

            // 2. cancel all pending requests
            var pendingRequestsSpec = new PendingRequestsByProviderSpec(provider.Id);
            var pendingRequests = await requestRepo.GetAllWithSpecAsync(pendingRequestsSpec);

            var notifications = new List<NotificationContentDto>();

            foreach (var request in pendingRequests)
            {
                request.Status = RequestStatus.Rejected;
                request.RejectionReason = "تم إلغاء الطلب بسبب حذف مقدم الخدمة لحسابه.";
                requestRepo.Update(request);

                notifications.Add(new NotificationContentDto
                {
                    UserId = request.Client.UserId,
                    Email = request.Client.User?.Email,
                    Subject = "إلغاء طلب خدمة ⚠️",
                    Body = $"نعتذر لك، تم إلغاء طلبك المعلق لخدمة '{request.Service.Title}' ...",
                    ReferenceId = request.Id
                });

            }

            // 3. delete all services and remove their images from disk
            var servicesSpec = new ActiveServicesByProviderSpec(provider.Id);
            var services = await serviceRepo.GetAllWithSpecAsync(servicesSpec);


            foreach(var service in services)
            {
                foreach(var image in service.Images)
                    await _attachmentService.DeleteFileAsync(image.ImageUrl);

                serviceRepo.Delete(service);
            }

            // 4. Delete provider files
            if (!string.IsNullOrEmpty(provider.ProfileImageUrl))
                await _attachmentService.DeleteFileAsync(provider.ProfileImageUrl);

            if (!string.IsNullOrEmpty(provider.CvUrl))
                await _attachmentService.DeleteFileAsync(provider.CvUrl);

            provider.User.IsActive = false;
            repo.Delete(provider);

            await _unitOfWork.SaveChangesAsync();

            

            foreach (var notification in notifications)
            {
                    await _notificationService.SendNotificationAsync(
                    notification,
                    NotificationType.Push,
                    NotificationType.Email
                );
            }
        }
    }
}

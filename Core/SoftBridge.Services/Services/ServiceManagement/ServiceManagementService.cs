using AutoMapper;
using E_commerce.Shared.Common.Dto.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SoftBridge.Abstraction.IServices.Attachement;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Abstraction.IServicesContract.Services;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Domain.Models.User;
using SoftBridge.Services.Specification.ProviderSpecifications;
using SoftBridge.Services.Specification.ServicesSpecifications;
using SoftBridge.Shared.Common.Dto.Attachement;
using SoftBridge.Shared.Common.Dto.Service;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Services.ServiceManagement
{
    public class ServiceManagementService(
        IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IAttachmentService _attachmentService,
        //INotificationService _notificationService,
        UserManager<ApplicationUser> _userManager 
        ) : IServiceManagement
    {
        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto createServiceDto, Guid providerId)
        {
            // 0. Validate Provider 
            await ValidateProviderAsync(providerId);

            // 1. Validate Category Exists and is Active
            await ValidateCategoryAsync(createServiceDto.CategoryId);

            var serviceEntity = _mapper.Map<Service>(createServiceDto);
            serviceEntity.ProviderId = providerId;
            serviceEntity.Status = ServiceStatus.Pending; // Default status

            // 3. Handle Image Uploads (Helper Method)
            if (createServiceDto.Images != null && createServiceDto.Images.Any())
            {
                serviceEntity.Images = await UploadServiceImagesAsync(createServiceDto.Images, providerId);
            }
            // 4. Save to Database
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();
            await serviceRepo.AddAsync(serviceEntity);
            await _unitOfWork.SaveChangesAsync();

            // 5. Notify Admins
            await NotifyAdminsForNewServiceAsync(serviceEntity);

            // 6. Return mapped DTO
            return _mapper.Map<ServiceDto>(serviceEntity);
        }

        public async Task<ServiceDto> UpdateServiceAsync(Guid serviceId, UpdateServiceDto updateServiceDto, Guid providerId)
        {
            // 1.Validate Provider
            await ValidateProviderAsync(providerId);
            // 2. Get the existing service with its images
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();
            var spec = new ServiceByIdWithImagesSpec(serviceId);
            var existingService = await serviceRepo.GetByIdWithSpecAsync(spec);
            // 3. Check existence and ownership
            if (existingService == null)
                throw new ServiceNotFoundException($"Service with not found.");
            if (existingService.ProviderId != providerId)
                throw new UnauthorizedExceptionCusotme();

            // 4. Validate Category
            if (existingService.CategoryId != updateServiceDto.CategoryId)
                await ValidateCategoryAsync(updateServiceDto.CategoryId);
            // 5. Map the updated fields onto the EXISTING entity
            _mapper.Map(updateServiceDto, existingService);
            // 6. Business Rule: Reset Status to Pending
            existingService.Status = ServiceStatus.Pending;
            // 7. Save changes
            serviceRepo.Update(existingService);
            await _unitOfWork.SaveChangesAsync();
            // 8. Return the updated DTO
            return _mapper.Map<ServiceDto>(existingService);
        }
       
        public async Task<PaginationResponse<ServiceDto>> GetProviderServicesAsync(Guid providerId, ServiceQueryParams queryParams)
        {
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();

            // 1. Spec for getting Data (With Pagination, Includes, OrderBy)
            var dataSpec = new ServiceWithFiltersForProviderSpec(providerId, queryParams);

            // 2. Spec for getting the Total Count (Only Filters)
            var countSpec = new ServiceCountForProviderSpec(providerId, queryParams);

            // 3. Execute Queries
            var services = await serviceRepo.GetAllWithSpecAsync(dataSpec);
            var totalItems = await serviceRepo.GetCountAsync(countSpec); 
            // return the total count of items that match the filters (without pagination) to calculate total pages on the client side

            // 4. Mapping
            var data = _mapper.Map<IReadOnlyList<ServiceDto>>(services);

            // 5. Return
            return new PaginationResponse<ServiceDto>(
                queryParams.PageIndex,
                queryParams.PageSize,
                totalItems,
                data
            );
        }

        public async Task<PaginationResponse<ServiceDto>> GetAllServicesAsync(ServiceQueryParams queryParams)
        {
            // to sure that the all services returned was approved
            queryParams.Status = ServiceStatus.Approved;

            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();

            // 1.spec for data
            var dataSpec = new ServiceWithFiltersSpec(queryParams);
            // spec for count
            var countSpec = new ServiceCountSpec(queryParams);

            //2- get from db
            var services = await serviceRepo.GetAllWithSpecAsync(dataSpec);
            var totalItems = await serviceRepo.GetCountAsync(countSpec);

            // 3. mapper
            var data = _mapper.Map<IReadOnlyList<ServiceDto>>(services);

            // 4.  Pagination
            return new PaginationResponse<ServiceDto>(
                queryParams.PageIndex,
                queryParams.PageSize,
                totalItems,
                data
            );
        }

        public async Task<bool> ChangeServiceStatusAsync(Guid serviceId, ServiceStatus status, string? rejectionReason = null)
        {
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();

            // 1. Get the service with its provider (to get the provider's user id for notification)
            var spec = new ServiceByIdWithProviderSpec(serviceId);
            var service = await serviceRepo.GetByIdWithSpecAsync(spec);

            if (service == null)
                throw new ServiceNotFoundException($"Service not found.");
            // IF admin tries to set the same status again, just ignore and return true (idempotent)
            if (service.Status == status)
                return true;
            // Business Rules
            if (status == ServiceStatus.Rejected && string.IsNullOrWhiteSpace(rejectionReason))
                throw new BadRequestExceptionCustome("Rejection reason is required when rejecting a service.");

            service.Status = status;
            if (status == ServiceStatus.Rejected)
                service.RejectionReason = rejectionReason;
            else
                service.RejectionReason = null;

            serviceRepo.Update(service);
            var result = await _unitOfWork.SaveChangesAsync() > 0;
            if (result)
                await NotifyProviderAboutServiceStatusAsync(service);
            return result;
        }

        public async Task<ServiceDetailsDto> GetServiceDetailsByIdAsync(Guid serviceId)
        {
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();

            var spec = new ServiceDetailsByIdSpec(serviceId);
            var service = await serviceRepo.GetByIdWithSpecAsync(spec);

            if (service == null)
                throw new ServiceNotFoundException($"Service not found.");

            return _mapper.Map<ServiceDetailsDto>(service);
        }

        public async Task<bool> DeleteServiceAsync(Guid serviceId, Guid providerId)
        {
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();

            var spec = new ServiceForDeletionSpec(serviceId);
            var service = await serviceRepo.GetByIdWithSpecAsync(spec);

            if (service == null)
                throw new ServiceNotFoundException($"Service not found.");

            if (service.ProviderId != providerId)
                throw new UnauthorizedExceptionCusotme();

            var hasActiveRequests = service.ServiceRequests.Any(req =>
                req.Status == RequestStatus.Pending ||
                req.Status == RequestStatus.Accepted);

            if (hasActiveRequests)
                throw new BadRequestExceptionCustome("Cannot delete this service because there are active requests from clients. You must complete or cancel them first.");

            // delete images from storage (Helper Method)
            if (service.Images != null && service.Images.Any())
                foreach (var image in service.Images)
                    await _attachmentService.DeleteFileAsync(image.ImageUrl);

            serviceRepo.Delete(service);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        #region Private Helper Methods for create and update services
        private async Task ValidateProviderAsync(Guid providerId)
        {
            var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();

            var spec = new ProviderByIdWithUserSpec(providerId);
            var provider = await providerRepo.GetByIdWithSpecAsync(spec);

            if (provider == null)
                throw new ProviderNotFoundException($"Provider profile not found.");

            if (provider.User == null || !provider.User.IsActive)
                throw new UnauthorizedExceptionCusotme();

            if (provider.Status != ProviderAccountStatus.Approved)
                throw new BadRequestExceptionCustome("Your account is not approved yet. You cannot create services until an admin approves your profile.");

        }
        private async Task ValidateCategoryAsync(Guid categoryId)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();
            var category = await categoryRepo.GetByIdAsync(categoryId);

            if (category == null)
                throw new CategoryNotFoundException($"Category with ID {categoryId} not found.");

            if (!category.IsActive)
                throw new BadRequestExceptionCustome("Cannot create a service in an inactive category.");
        }
        private async Task<ICollection<ServiceImage>> UploadServiceImagesAsync(List<IFormFile> images, Guid providerId)
        {
            var uploadedImages = new List<ServiceImage>();
            int order = 1;

            foreach (var file in images)
            {
                var uploadDto = new UploadFileDto
                {
                    File = file,
                    FolderName = Path.Combine("Services", providerId.ToString())
                };

                var imagePath = await _attachmentService.UploadFileAsync(uploadDto);

                uploadedImages.Add(new ServiceImage
                {
                    ImageUrl = imagePath,
                    IsPortfolio = order > 1, // if first image so it not protofolio its main image
                    DisplayOrder = order++
                });
            }

            return uploadedImages;
        }
        private async Task NotifyAdminsForNewServiceAsync(Service service)
        {
            // get all admins
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            foreach (var admin in admins)
            {
                // send notification and email
            }
        }
        private async Task NotifyProviderAboutServiceStatusAsync(Service service)
        {
            string title = string.Empty;
            string body = string.Empty;

            if (service.Status == ServiceStatus.Approved)
            {
                title = "Service Approved! 🎉";
                body = $"Congratulations! Your service '{service.Title}' has been approved and is now live for clients.";
            }
            else if (service.Status == ServiceStatus.Rejected)
            {
                title = "Service Rejected ⚠️";
                body = $"Your service '{service.Title}' requires modifications. Reason: {service.RejectionReason}";
            }
            else
            {
                // if the status is pending or any other status, we might choose not to send a notification
                return;
            }

            // Notification
        }
        #endregion

    }
}

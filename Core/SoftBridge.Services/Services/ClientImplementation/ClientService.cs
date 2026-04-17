using AutoMapper;
using SoftBridge.Abstraction.IServices.Attachement;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Domain.Contracts.SpecificationPattern.ClientSpec;
using SoftBridge.Domain.Contracts.SpecificationPattern.ServiceRequestSpec;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Shared.Common.Dto.Attachement;
using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Shared.Common.Dto.Review;
using SoftBridge.Shared.Common.Dto.ServiceRequest;

namespace SoftBridge.Services.Services.ClientImplementation
{
    public class ClientService: IClientProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;
        public ClientService(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
        }

        public async Task<ClientProfileDto> GetMyProfileAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<Client, Guid>();
            var clientSpec = new ClientByUserIdSpec(userId);
            var client = await repo.GetByIdWithSpecAsync(clientSpec);

            if (client is null)
            {
                throw new ClientNotFoundException(
                    $"No client profile found for user.");
            }
            return _mapper.Map<ClientProfileDto>(client);
        }

        public async Task<ClientProfileDto> GetByIdAsync(Guid clientId)
        {
            var repo = _unitOfWork.GetRepository<Client, Guid>();
            var clientSpec = new ClientByIdSpec(clientId);
            var client = await repo.GetByIdWithSpecAsync(clientSpec);

            if (client is null)
                throw new ClientNotFoundException(
                    $"Client with id '{clientId}' was not found.");

            return _mapper.Map<ClientProfileDto>(client);
        }
        public async Task<ClientProfileDto> UpdateProfileAsync(string userId, UpdateClientProfileDto updateDto)
        {
            var repo = _unitOfWork.GetRepository<Client, Guid>();
            var spec = new ClientByUserIdSpec(userId);
            var client = await repo.GetByIdWithSpecAsync(spec);

            if (client is null)
                throw new ClientNotFoundException(
                    $"No client profile found for user.");

            client.User.FullName = updateDto.FullName;

            
            // handle profile picture
            if (updateDto.ProfileImageUrl != null)
            {
                var folder = Path.Combine("Providers", userId, "Profile");
                var imagePath = await _attachmentService.UploadFileAsync(new UploadFileDto
                {
                    File = updateDto.ProfileImageUrl,
                    FolderName = folder
                });

                if (!string.IsNullOrEmpty(client.ProfileImageUrl))
                    await _attachmentService.DeleteFileAsync(client.ProfileImageUrl);

                client.ProfileImageUrl = imagePath;
            }

            repo.Update(client);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClientProfileDto>(client);
        }

        public async Task DeleteAccountAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<Client, Guid>();
            var spec = new ClientByUserIdSpec(userId);
            var client = await repo.GetByIdWithSpecAsync(spec);

            if (client is null)
                throw new ClientNotFoundException(
                    $"No client profile found for user.");

            client.User.IsActive = false;

            repo.Delete(client);

            await _unitOfWork.SaveChangesAsync();
        }

        //public async Task<IReadOnlyList<ServiceRequestDto>> GetMyRequestsAsync(Guid clientId)
        //{
        //    var repo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
        //    var spec = new ClientRequestsSpec(clientId);
        //    var requests = await repo.GetAllWithSpecAsync(spec);

        //    return _mapper.Map<IReadOnlyList<ServiceRequestDto>>(requests);
        //}

        //public async Task<ServiceRequestDto> GetRequestByIdAsync(Guid requestId, Guid clientId)
        //{
        //    var repo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
        //    var spec = new ClientRequestByIdSpec(requestId, clientId);
        //    var request = await repo.GetByIdWithSpecAsync(spec);

        //    if (request is null)
        //        throw new ClientNotFoundException(
        //            $"Request '{requestId}' was not found.");

        //    return _mapper.Map<ServiceRequestDto>(request);
        //}

    }
}

using SoftBridge.Shared.Common.Dto.ServiceProvider;
using SoftBridge.Domain.Models.EnumHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Abstraction.IServices.Profiles
{
    // This interface manages the Service Provider's profile, 
    // including their bio, CV, portfolio links, and aggregated ratings.
    public interface IProviderProfileService
    {
        //Task<ProviderProfileDto> GetProfileAsync(Guid providerId); // Can be viewed by Clients and Admins. Should include aggregated rating and reviews, but not sensitive info like CV details.
        //Task<ProviderProfileDto> UpdateProfileAsync(Guid providerId, UpdateProviderProfileDto updateDto);
        //Task<bool> UpdateAccountStatusAsync(Guid providerId, AccountStatus status); 

        #region Manage Profile
        Task<ProviderProfileDto> GetMyProfileAsync(string userId);
        Task<ProviderProfileDto> UpdateProfileAsync(string userId, UpdateProviderProfileDto updateDto);
        Task DeleteAccountAsync(string userId);
        #endregion

        #region Manage Services
        Task<IReadOnlyList<ServiceWithProviderDto>> GetMyServicesAsync(string userId);
        Task<ServiceWithProviderDto> AddServiceAsync(string userId, AddServiceWithProviderDto dto);
        Task<ServiceWithProviderDto> UpdateServiceAsync(string userId, Guid serviceId, UpdateServiceWithProviderDto dto);
        Task DeleteServiceAsync(string userId, Guid serviceId);
        #endregion

        #region Manage Requests
        Task<IReadOnlyList<IncomingRequestDto>> GetIncomingRequestsAsync(string userId, RequestStatus? status);
        Task<IncomingRequestDto> RespondToRequestAsync(string userId, Guid requestId, RespondToRequestDto dto);
        Task<IncomingRequestDto> AcceptRequestAsync(string userId, Guid requestId, RespondToRequestDto dto);
        Task<IncomingRequestDto> RejectRequestAsync(string userId, Guid requestId, RespondToRequestDto dto);
        #endregion

        #region Manage Reviews
        Task<IReadOnlyList<ReceivedReviewDto>> GetMyReviewsAsync(string userId);
        #endregion
    }
}
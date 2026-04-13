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
    }
}
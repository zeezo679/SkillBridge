using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Shared.Common.Dto.Review;
using SoftBridge.Shared.Common.Dto.ServiceRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Abstraction.IServices.Profiles
{
    // This interface handles the Client's personal profile information.
    public interface IClientProfileService
    {
        Task<ClientProfileDto> GetMyProfileAsync(string userId);
        Task<ClientProfileDto> GetByIdAsync(Guid clientId);
        Task<ClientProfileDto> UpdateProfileAsync(string userId, UpdateClientProfileDto updateDto);
        Task DeleteAccountAsync(string userId);
        //Task<IReadOnlyList<ServiceRequestDto>> GetMyRequestsAsync(Guid clientId);
        //Task<ServiceRequestDto> GetRequestByIdAsync(Guid requestId, Guid clientId);
    }
}
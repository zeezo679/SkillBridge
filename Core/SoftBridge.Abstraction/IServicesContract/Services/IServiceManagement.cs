using E_commerce.Shared.Common.Dto.Service;
using SoftBridge.Shared.Common.Dto.Service;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Abstraction.IServicesContract.Services
{
    // This interface defines the contract for service management.
    // Handles CRUD operations for Providers, monitoring for Admins, and browsing for Clients.
    public interface IServiceManagement
    {
        // --- Provider Operations ---
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto createServiceDto, Guid providerId);
        Task<ServiceDto> UpdateServiceAsync(Guid serviceId, UpdateServiceDto updateServiceDto, Guid providerId);
        //Task<bool> DeleteServiceAsync(Guid serviceId, Guid providerId);
        Task<PaginationResponse<ServiceDto>>GetProviderServicesAsync(Guid providerId, ServiceQueryParams queryParams);

        // --- Shared (Admin & Client) Operations ---
        // Retrieves services based on filters. 
        // Clients will filter by Status=Approved. Admins can filter by Status=Pending to review.
        //Task<Pagination<ServiceDto>> GetAllServicesAsync(ServiceQueryParams queryParams); 
        //Task<ServiceDetailsDto> GetServiceDetailsByIdAsync(Guid serviceId);

        // --- Admin Operations ---
        //Task<bool> ChangeServiceStatusAsync(Guid serviceId, ServiceStatus status, string? rejectionReason = null);
    }
}

// --- Query Params Classes ---
/*
 * // --- Query Params Classes (To be placed in  Shared/Common/Params/Service folder) ---
public class ServiceQueryParams : BaseQueryParams
{
    public string? SearchTerm { get; set; } // Search by Title or Description
    public int? CategoryId { get; set; } // Filter by Category
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ServiceStatus? Status { get; set; } // Pending, Approved, Rejected
}
*/
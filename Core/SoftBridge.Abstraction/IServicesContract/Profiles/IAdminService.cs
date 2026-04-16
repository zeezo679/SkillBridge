using System;
using System.Collections.Generic;
using System.Text;
using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Admin;

namespace SoftBridge.Abstraction.IServices.Profiles
{
    public interface IAdminService
    {
        /// <summary>
        /// Gets all service providers with filtering options.
        /// </summary>
        /// <param name="queryParams">The query parameters for filtering and pagination.</param>
        /// <returns>A paginated list of provider profiles.</returns>
        Task<PaginationResponse<ProviderProfileDto>> GetAllProvidersAsync(ProviderQueryParams queryParams);

        // Retrieves all clients with filtering (SearchTerm, Active/Inactive)
        /// <summary>
        /// Gets all clients with filtering options.
        /// </summary>
        /// <param name="queryParams">The query parameters for filtering and pagination.</param>
        /// <returns>A paginated list of client profiles.</returns>
        Task<PaginationResponse<ClientProfileDto>> GetAllClientsAsync(ClientQueryParams queryParams);

        /// <summary>
        /// Approves a service provider's account, changing their status to "Approved".
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        Task ApproveProviderAsync(string providerId, string adminId);
    
        /// <summary>
        /// Rejects a service provider's account, changing their status to "Rejected" and providing
        /// a reason for the rejection.
        /// </summary>
        Task RejectProviderAsync(string providerId, string adminId);
    }
}

// --- Query Params Classes (To be placed in  Shared/Common/Admin folder) ---
/*
public class ClientQueryParams : BaseQueryParams
{
    public string? SearchTerm { get; set; } // Search by name or email
    public bool? IsActive { get; set; } // Filter by active/banned clients
}

// This class extends BaseQueryParams to add filtering specific to Service Providers
    public class ProviderQueryParams : BaseQueryParams
    {
        // 1. Search: To search by Provider's Name, Email, or Phone Number
        public string? SearchTerm { get; set; }

        // 2. Status Filter: To get Pending, Approved, or Rejected providers
        public AccountStatus? Status { get; set; }

        // 3. Verification Filter: Based on the "IsVerified" feature we added earlier
        public bool? IsVerified { get; set; }

        // 4. Rating Filter: To fetch top-rated providers (e.g., >= 4.0)
        public float? MinRating { get; set; }

        // 5. Date Range Filters: To get providers registered in a specific month/year
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }
    }
*/
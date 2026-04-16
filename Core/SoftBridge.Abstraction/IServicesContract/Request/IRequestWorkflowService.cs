using SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Requests;

namespace SoftBridge.Abstraction.IServicesContract.Request
{
    // This interface defines the contract for the service request workflow.
    // Handles the lifecycle of a request from creation to completion, and platform monitoring.
    public interface IRequestWorkflowService
    {
        // --- Client Operations ---
        Task<RequestDto> SendServiceRequestAsync(CreateRequestDto createRequestDto, Guid clientId);
        Task<PaginationResponse<RequestDto>> GetClientRequestsAsync(Guid clientId , RequestQueryParams queryParams); // Client can view all their requests with filtering options 

        // --- Provider Operations ---
        Task<PaginationResponse<RequestDto>> GetProviderRequestsAsync(Guid providerId, RequestQueryParams queryParams); // Provider can view all requests assigned to them with filtering options 
        Task<bool> AcceptRequestAsync(Guid requestId, Guid providerId);
        Task<bool> RejectRequestAsync(Guid requestId, Guid providerId, string rejectionReason);

        // --- Shared Operations ---
        Task<bool> CompleteRequestAsync(Guid requestId, Guid userId); 
        Task<RequestDetailsDto> GetRequestDetailsAsync(Guid requestId);

        // --- Admin Operations ---
        Task<PaginationResponse<RequestDto>> GetAllPlatformRequestsAsync(RequestQueryParams queryParams); // Admin can monitor all requests
    }
}

// --- Query Params Classes ---
/*
 * // --- Query Params Classes (To be placed in  Shared/Common/Requests folder) ---
public class RequestQueryParams : BaseQueryParams
{
    public RequestStatus? Status { get; set; } // Pending, Accepted, Rejected, Completed
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
*/
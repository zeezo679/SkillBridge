using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Request;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Services.Services.ClientImplementation;
using SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos;
using SoftBridge.Shared.Common.Params.Requests;
using System.Security.Claims;

namespace SoftBridge.Web.Controllers.ServiceRequestController
{
    public class RequestController : AppBaseController
    {
        private readonly IRequestWorkflowService _requestService;
        private readonly IClientProfileService _clientService;
        private readonly IProviderProfileService _providerService;

        public RequestController(IRequestWorkflowService requestService,
                                IClientProfileService clientService,
                                IProviderProfileService providerService)
        {
            _requestService = requestService;
            _clientService = clientService;
            _providerService = providerService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Client Endpoints

        // POST api/request
        // Client sends a new service request.
        // Blocked if client already has an active request for the same service.
        [HttpPost]
        [Authorize(Roles ="Client")]
        public async Task<IActionResult> SendRequest([FromBody] CreateRequestDto dto)
        {
            var client = await _clientService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.SendServiceRequestAsync(dto, client.Id);

            return Created(result, "Request sent successfully.");
        }

        // GET api/request/client?status=Pending&pageIndex=1&pageSize=10
        // Client views all their own requests.
        // Supports filtering by Status, FromDate, ToDate, Search, and pagination.
        [HttpGet("client")]
        [Authorize(Roles ="Client")]
        public async Task<IActionResult> GetClientRequests([FromQuery] RequestQueryParams queryParams)
        {
            var client = await _clientService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.GetClientRequestsAsync(client.Id, queryParams);

            return Success(result);
        }


        // Provider Endpoints

        //GET api/request/provider? status = Pending & pageIndex = 1 & pageSize = 10
        [HttpGet("Provider")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetProviderRequests(
        [FromQuery] RequestQueryParams queryParams)
        {
            var provider = await _providerService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService
                                 .GetProviderRequestsAsync(provider.Id, queryParams);
            return Success(result);
        }

        // PUT api/request/{id}/accept
        // Provider accepts a pending request.
        [HttpPut("{id:guid}/accept")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Accept(Guid id)
        {
            var provider = await _providerService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.AcceptRequestAsync(id, provider.Id);

            return Success(result, "Request accepted successfully.");
        }

        // PUT api/request/{id}/reject
        [HttpPut("{id:guid}/reject")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string rejectionReason)
        {
            var provider = await _providerService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.RejectRequestAsync(id, provider.Id, rejectionReason);

            return Success(result, "Request rejected.");
        }

        // GET api/request/{id}
        // Returns full details of a single request.
        // Accessible by the client, provider, or admin involved.
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            var result = await _requestService.GetRequestDetailsAsync(id);
            return Success(result);
        }

        // PUT api/request/{id}/complete
        // Provider marks an accepted request as completed.
        // Only possible when status is Accepted.
        // After this the client can submit a review.
        [HttpPut("{id:guid}/complete")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> CompleteRequest(Guid id)
        {
            var provider = await _providerService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.CompleteRequestAsync(id, provider.Id);

            return Success(result, "Request marked as completed.");
        }


        // GET api/request/admin
        // Admin monitors all platform requests.
        // Supports filtering by Status, FromDate, ToDate, Search,pagination, Client and Provider name.
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPlatformRequests([FromQuery] RequestQueryParams queryParams)
        {
            var result = await _requestService.GetAllPlatformRequestsAsync(queryParams);

            return Success(result);
        }
    }
}

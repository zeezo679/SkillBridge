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

        // POST api/request/me
        [HttpPost("me")]
        [Authorize(Roles ="Client")]
        public async Task<IActionResult> SendRequest([FromBody] CreateRequestDto dto)
        {
            var client = await _clientService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.SendServiceRequestAsync(dto, client.Id);

            return Success(result);
        }

        // GET api/requests/client?status=Pending&pageIndex=1&pageSize=10
        [HttpGet("client")]
        [Authorize(Roles ="Client")]
        public async Task<IActionResult> GetClientRequests([FromQuery] RequestQueryParams queryParams)
        {
            var client = await _clientService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.GetClientRequestsAsync(client.Id, queryParams);

            return Success(result);
        }


        // Provider Endpoints

        //GET api/requests/provider? status = Pending & pageIndex = 1 & pageSize = 10
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

        // PUT api/requests/{id}/accept
        [HttpPut("{id:guid}/accept")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Accept(Guid id)
        {
            var provider = await _providerService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.AcceptRequestAsync(id, provider.Id);

            return Success(result, "Request accepted successfully.");
        }

        // PUT api/requests/{id}/reject
        [HttpPut("{id:guid}/reject")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string rejectionReason)
        {
            var provider = await _providerService.GetMyProfileAsync(CurrentUserId);
            var result = await _requestService.RejectRequestAsync(id, provider.Id, rejectionReason);

            return Success(result, rejectionReason);
        }
    }
}

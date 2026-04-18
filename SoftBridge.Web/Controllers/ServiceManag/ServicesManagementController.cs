using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServicesContract.Services;
using SoftBridge.Services.Services.ServiceManagement;
using SoftBridge.Shared.Common.Dto.Service;
using SoftBridge.Shared.Common.Params.Service;
using System.Security.Claims;

namespace SoftBridge.Web.Controllers.ServiceManag
{
    public class ServicesController(IServiceManagement serviceManagement) : AppBaseController
    {
        // get the user id from the token
        private Guid GetUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 1. (Public Operations)

        [HttpGet]
        [AllowAnonymous] // anyone can view the services without authentication
        public async Task<IActionResult> GetAllServices([FromQuery] ServiceQueryParams queryParams)
        {
            var result = await serviceManagement.GetAllServicesAsync(queryParams);
            return Success(result, "Services retrieved successfully");
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceDetails(Guid id)
        {
            var result = await serviceManagement.GetServiceDetailsByIdAsync(id);
            return Success(result, "Service details retrieved successfully");
        }

        [HttpGet("provider/{providerId:guid}")]
        [AllowAnonymous] // to allow clients to view services of a specific provider without authentication
        public async Task<IActionResult> GetProviderServices(Guid providerId, [FromQuery] ServiceQueryParams queryParams)
        {
            var result = await serviceManagement.GetProviderServicesAsync(providerId, queryParams);
            return Success(result, "Provider services retrieved successfully");
        }

        // 2. (Provider Operations)

        [HttpPost]
        [Authorize(Roles = "Provider")] // only providers can create services
        public async Task<IActionResult> CreateService([FromForm] CreateServiceDto createServiceDto)
        {
            var providerId = GetUserId();
            var result = await serviceManagement.CreateServiceAsync(createServiceDto, providerId);
            return Created(result, "Service created successfully and is pending admin approval.");
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceDto updateServiceDto)
        {
            var providerId = GetUserId();
            var result = await serviceManagement.UpdateServiceAsync(id, updateServiceDto, providerId);
            return Success(result, "Service updated successfully and returned to pending status.");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var providerId = GetUserId();
            await serviceManagement.DeleteServiceAsync(id, providerId);
            return Success("Service and its images deleted successfully.");
        }

        // 3.(Admin Operations)

        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Admin")] // only admins can change the status of a service
        public async Task<IActionResult> ChangeServiceStatus(Guid id, [FromBody] ChangeServiceStatusDto statusDto)
        {
            await serviceManagement.ChangeServiceStatusAsync(id, statusDto.Status, statusDto.RejectionReason);
            return Success($"Service status changed to {statusDto.Status} successfully.");
        }
    }
}

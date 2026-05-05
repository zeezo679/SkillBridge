using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServicesContract.Services;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Shared.Common.Dto.Service;
using SoftBridge.Shared.Common.Params.Service;
using System.Security.Claims;

namespace SoftBridge.Web.Controllers.ServiceManag
{
    [Route("api/[controller]")]
    public class ServicesController(IServiceManagement serviceManagement) : AppBaseController
    {
        private Guid GetUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ==========================================
        // 1. (Public Operations)
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        [Tags("1. Public Services")] 
        public async Task<IActionResult> GetAllServices([FromQuery] ServiceQueryParams queryParams)
        {
            var isAdmin = User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            if (!isAdmin)
                queryParams.Status = ServiceStatus.Approved;
            var result = await serviceManagement.GetAllServicesAsync(queryParams);
            return Success(result, "Services retrieved successfully");
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [Tags("1. Public Services")]
        public async Task<IActionResult> GetServiceDetails(Guid id)
        {
            var result = await serviceManagement.GetServiceDetailsByIdAsync(id);
            return Success(result, "Service details retrieved successfully");
        }

        [HttpGet("provider/{providerId:guid}")]
        [AllowAnonymous]
        [Tags("1. Public Services")]
        public async Task<IActionResult> GetProviderServices(Guid providerId, [FromQuery] ServiceQueryParams queryParams)
        {
            var result = await serviceManagement.GetProviderServicesAsync(providerId, queryParams);
            return Success(result, "Provider services retrieved successfully");
        }

        // ==========================================
        // 2. (Provider Operations)
        // ==========================================

        [HttpPost]
        [Authorize(Roles = "Provider")]
        [Tags("2. Provider Services")] 
        public async Task<IActionResult> CreateService([FromForm] CreateServiceDto createServiceDto)
        {
            var userId   = GetUserId();
            var result = await serviceManagement.CreateServiceAsync(createServiceDto, userId    );
            return Created(result, "Service created successfully and is pending admin approval.");
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Provider")]
        [Tags("2. Provider Services")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceDto updateServiceDto)
        {
            var userId = GetUserId();
            var result = await serviceManagement.UpdateServiceAsync(id, updateServiceDto, userId);
            return Success(result, "Service updated successfully and returned to pending status.");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Provider")]
        [Tags("2. Provider Services")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var userId = GetUserId();
            await serviceManagement.DeleteServiceAsync(id, userId);
            return Success("Service and its images deleted successfully.");
        }

        // ==========================================
        // 3. (Admin Operations)
        // ==========================================

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [Tags("3. Admin Services")]
        public async Task<IActionResult> GetAdminServices([FromQuery] ServiceQueryParams queryParams)
        {
            var result = await serviceManagement.GetAdminServicesAsync(queryParams);
            return Success(result, "Services retrieved successfully");
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Admin")]
        [Tags("3. Admin Services")] 
        public async Task<IActionResult> ChangeServiceStatus(Guid id, [FromBody] ChangeServiceStatusDto statusDto)
        {
            await serviceManagement.ChangeServiceStatusAsync(id, statusDto.Status, statusDto.RejectionReason);
            return Success($"Service status changed to {statusDto.Status} successfully.");
        }
    }
}
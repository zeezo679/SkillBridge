using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Admin;
using SoftBridge.Shared.Common.Responses;

namespace SoftBridge.Web.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService) : AppBaseController
{
    [HttpGet("providers")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<ProviderProfileDto>>>> GetProviders([FromQuery] ProviderQueryParams queryParams)
    {
        var result = await adminService.GetAllProvidersAsync(queryParams);

        return Success<PaginationResponse<ProviderProfileDto>>(result, "Providers retrieved successfully");
    }

    // GET api/admin/clients?IsActive=true
    [HttpGet("clients")]
    public async Task<IActionResult> GetClients([FromQuery] ClientQueryParams queryParams)
    {
        var result = await adminService.GetAllClientsAsync(queryParams);

        return Success<PaginationResponse<ClientProfileDto>>(result, "Clients retrieved successfully");
    }

    [HttpPatch("providers/{providerId}/approve")]
    public async Task<IActionResult> ApproveProvider([FromRoute] string providerId)
    {
        string admin = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await adminService.ApproveProviderAsync(providerId, admin);

        return Success("Provider approved successfully");
    }

    [HttpPatch("providers/{providerId}/reject")]
    public async Task<IActionResult> RejectProvider([FromRoute] string providerId)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await adminService.RejectProviderAsync(providerId, adminId);

        return Success("Provider rejected successfully");
    }
}

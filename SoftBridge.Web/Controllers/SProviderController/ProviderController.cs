using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Request;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using System.Security.Claims;

namespace SoftBridge.Web.Controllers.SProviderController
{

    [Authorize(Roles ="Provider")]
    public class ProviderController : AppBaseController
    {
        private readonly IProviderProfileService _providerService;
        public ProviderController(IProviderProfileService providerService,
                                        IRequestWorkflowService requestService)
        {
            _providerService = providerService;
        }


        // reads the NameIdentifier claim set by Identity JWT
        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Profile

        // GET api/provider/me
        // Returns the profile of the currently logged-in provider.
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _providerService.GetMyProfileAsync(CurrentUserId);
            return Success(result);
        }

        // PUT api/provider/me
        // Updates FullName, Bio, PortfolioLink, ProfilePicture, and CV.
        [HttpPut("me")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProviderProfileDto dto)
        {
            var result = await _providerService.UpdateProfileAsync(CurrentUserId, dto);
            return Success(result, "Profile updated successfully.");
        }

        // DELETE api/provider/me
        // Soft-deletes the provider account.
        // Blocked if there are active (Accepted) requests.
        // Cancels all pending requests and soft-deletes all services first.
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteAccount()
        {
            await _providerService.DeleteAccountAsync(CurrentUserId);
            return Success("Account deleted successfully.");
        }
    }
}

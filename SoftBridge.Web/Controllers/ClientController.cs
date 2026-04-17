using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Request;
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Shared.Common.Dto.Client;
using System.Security.Claims;

namespace SoftBridge.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : AppBaseController
    {
        private readonly IClientProfileService _clientService;
        private readonly IRequestWorkflowService _requestService;
        private readonly IReviewService _reviewService;

        public ClientController(
            IClientProfileService clientService,
            IRequestWorkflowService requestService,
            IReviewService reviewService)
        {
            _clientService = clientService;
            _requestService = requestService;
            _reviewService = reviewService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET api/client/me
        // Returns the profile of the currently logged-in client.
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _clientService.GetMyProfileAsync(CurrentUserId);

            return Success(result);
        }

        // PUT api/client/me
        // Updates FullName and optionally replaces the profile picture.
        [HttpPut("me")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateClientProfileDto dto)
        {
            var result = await _clientService.UpdateProfileAsync(CurrentUserId, dto);

            return Success(result, "Profile updated successfully.");
        }

        // DELETE api/client/me
        // Soft-deletes the client account and deactivates the user.
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteAccount()
        {
            await _clientService.DeleteAccountAsync(CurrentUserId);

            return Success("Account deleted successfully.");
        }
    }
}

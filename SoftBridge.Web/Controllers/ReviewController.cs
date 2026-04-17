using E_commerce.Shared.Common.Dto.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Shared.Common.Dto.Review;
using System.Security.Claims;

namespace SoftBridge.Web.Controllers
{
    public class ReviewController : AppBaseController
    {
        private readonly IReviewService _reviewService;
        private readonly IClientProfileService _clientService;

        public ReviewController(
            IReviewService reviewService,
            IClientProfileService clientService)
        {
            _reviewService = reviewService;
            _clientService = clientService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET api/review/{id}
        // Returns a single review by its ID.
        // Accessible by anyone.
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _reviewService.GetReviewByIdAsync(id);

            return Success(result);
        }

        // GET api/review/service/{serviceId}
        // Returns all reviews for a specific service.
        // Used on the public service detail page.

        [HttpGet("service/{serviceId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByService(Guid serviceId)
        {
            var result = await _reviewService.GetReviewsByServiceAsync(serviceId);

            return Success(result);
        }

        // GET api/reviews/provider/{providerId}
        // Returns all reviews received by a specific provider.
        // Used on the public provider profile page.
        [HttpGet("provider/{providerId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProvider(Guid providerId)
        {
            var result = await _reviewService.GetReviewsByProviderAsync(providerId);

            return Success(result);
        }

        // GET api/review/me
        // Returns all reviews written by the currently logged-in client.
        [HttpGet("me")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> GetMyReviews()
        {
            var result = await _reviewService.GetMyReviewsAsync(CurrentUserId);
            return Success(result);
        }

        // POST api/review
        // Client submits a review for a completed request.
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDto dto)
        {
            var result = await _reviewService.AddReviewAsync(CurrentUserId, dto);

            return Success(result, "Review submitted successfully.");
        }

        // PUT api/review/{id}
        // Client updates their own review.
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewDto dto)
        {
            var result = await _reviewService.UpdateReviewAsync(CurrentUserId, id, dto);

            return Success(result, "Review updated successfully.");
        }

        // DELETE api/review/{id}
        // Client deletes their own review.
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            await _reviewService.DeleteReviewAsync(CurrentUserId, id);

            return Success("Review deleted successfully.");
        }
    }
}

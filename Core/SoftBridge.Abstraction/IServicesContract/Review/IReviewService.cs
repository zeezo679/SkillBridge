using SoftBridge.Shared.Common.Dto.Review;
using SoftBridge.Shared.Common.Params;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Abstraction.IServicesContract.Review
{
    // This interface defines the contract for the review service.
    public interface IReviewService
    {
        //Task<ReviewDto> AddReviewAsync(CreateReviewDto createReviewDto, Guid clientId);

        // Retrieves reviews for a specific service, filtered by rating or sorted by date.
        //Task<Pagination<ReviewDto>> GetServiceReviewsAsync(Guid serviceId);

        //Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId); // Admin or the Client who wrote it

        Task<ReviewDto> AddReviewAsync(Guid clientId, AddReviewDto dto);
    }
}


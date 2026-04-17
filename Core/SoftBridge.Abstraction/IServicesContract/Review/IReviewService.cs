using E_commerce.Shared.Common.Dto.Review;
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



        // client submits a review after a completed request
        Task<ReviewDto> AddReviewAsync(string userId, AddReviewDto dto);

        // client updates their own review
        Task<ReviewDto> UpdateReviewAsync(string userId, Guid reviewId, UpdateReviewDto dto);

        // client deletes their own review
        Task DeleteReviewAsync(string userId, Guid reviewId);

        // get a single review by its id — public
        Task<ReviewDto> GetReviewByIdAsync(Guid reviewId);

        // all reviews written by this client
        Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(string userId);

        // all reviews for a specific service
        Task<IReadOnlyList<ReviewDto>> GetReviewsByServiceAsync(Guid serviceId);
        Task<IReadOnlyList<ReviewDto>> GetReviewsByProviderAsync(Guid providerId);

    }
}





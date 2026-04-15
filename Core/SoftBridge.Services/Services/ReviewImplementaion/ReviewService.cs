using AutoMapper;
using SoftBridge.Abstraction.IServices.Attachement;
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Domain.Contracts.SpecificationPattern.ServiceRequestSpec;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Shared.Common.Dto.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.Services.ReviewImplementaion
{
    public class ReviewService: IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;
        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
        }
        public async Task<ReviewDto> AddReviewAsync(Guid clientId, AddReviewDto dto)
        {
            // ── 1. validate rating range
            if (dto.Rating is < 1 or > 5)
                throw new ReviewBadRequestException(
                    "Rating must be between 1 and 5.");

            // ── 2. load the request with its current review ──────────────────────
            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var spec = new RequestForReviewSpec(dto.RequestId, clientId);
            var request = await requestRepo.GetByIdWithSpecAsync(spec);

            if (request is null)
                throw new ClientNotFoundException(
                    $"Request '{dto.RequestId}' was not found.");

            // ── 3. request must be completed before reviewing ────────────────────
            if (request.Status != RequestStatus.Completed)
                throw new ReviewBadRequestException(
                    "You can only review a completed request.");

            // ── 4. prevent duplicate reviews ─────────────────────────────────────
            if (request.Review is not null)
                throw new ReviewBadRequestException(
                    "You have already submitted a review for this request.");

            // ── 5. build the Review entity ───────────────────────────────────────
            var review = new Review
            {
                Id = Guid.NewGuid(),
                RequestId = request.Id,
                ClientId = clientId,
                ProviderId = request.ProviderId,
                ServiceId = request.ServiceId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            // ── 6. persist the review ────────────────────────────────────────────
            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            await reviewRepo.AddAsync(review);

            // ── 7. update Provider's AverageRating and TotalReviews ──────────────
            var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();
            var provider = await providerRepo.GetByIdAsync(request.ProviderId);

            if (provider is not null)
            {
                provider.TotalReviews++;
                // incremental average: newAvg = oldAvg + (newRating - oldAvg) / totalReviews
                // avoids reloading all reviews from DB just to recalculate
                provider.AverageRating += (dto.Rating - provider.AverageRating)
                                          / provider.TotalReviews;
                providerRepo.Update(provider);
            }

            // ── 8. update Service's AverageRating ────────────────────────────────
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();
            var service = await serviceRepo.GetByIdAsync(request.ServiceId);

            if (service is not null)
            {
                // reuse the same incremental formula
                // we don't track TotalReviews on Service so derive it from provider
                // alternatively store a counter on Service — add if needed
                var oldRating = service.AverageRating;
                var reviewCount = provider?.TotalReviews ?? 1;
                service.AverageRating += (dto.Rating - oldRating) / reviewCount;
                serviceRepo.Update(service);
            }

            // ── 9. commit everything in one transaction ───────────────────────────
            await _unitOfWork.SaveChangesAsync();

            // ── 10. reload with navigations for the response DTO ─────────────────
            // review.ServiceRequest is not loaded yet — use the request we already have
            review.ServiceRequest = request;
            review.Provider = request.Provider;

            return _mapper.Map<ReviewDto>(review);
        }

    }
}

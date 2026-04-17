using AutoMapper;
using E_commerce.Shared.Common.Dto.Review;
using SoftBridge.Abstraction.IServices.Attachement;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Notification;
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Exceptions;
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.EnumHelper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Services.Specification.ReviewSpecifications;
using SoftBridge.Shared.Common.Dto.Notification;
using SoftBridge.Shared.Common.Dto.Review;

namespace SoftBridge.Services.Services.ReviewImplementaion
{
    public class ReviewService: IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;
        private readonly IClientProfileService _clientService;
        private readonly INotificationService _notificationService;
        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService,
                IClientProfileService clientService , INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
            _clientService = clientService;
            _notificationService = notificationService;
        }

        public async Task<ReviewDto> AddReviewAsync(string userId, AddReviewDto dto)
        {
            var clientDto = await _clientService.GetMyProfileAsync(userId);

            if (dto.Rating is < 1 or > 5)
                throw new ReviewBadRequestException("Rating must be between 1 and 5");

            var requestRepo = _unitOfWork.GetRepository<ServiceRequest, Guid>();
            var requestSpec = new RequestForReviewSpec(dto.RequestId, clientDto.Id);
            var request = await requestRepo.GetByIdWithSpecAsync(requestSpec);

            if (request == null)
                throw new ReviewNotFoundException("Service request not found or you are not authorized to review this request");

            if (request.Status != RequestStatus.Completed)
                throw new ReviewBadRequestException("You can only review completed service requests");

            if (request.Review != null)
                throw new ReviewBadRequestException("You have already reviewed this service request");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                RequestId = request.Id,
                ClientId = clientDto.Id,
                ProviderId = request.ProviderId,
                ServiceId = request.ServiceId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            await reviewRepo.AddAsync(review);

            // update Provider AverageRating — incremental formula
            // avoids reloading all reviews just to recalculate

            var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();
            var provider = await providerRepo.GetByIdAsync(request.ProviderId);

            if (provider != null)
            {
                provider.TotalReviews++;
                provider.AverageRating += (dto.Rating - provider.AverageRating) / (float)provider.TotalReviews;

                providerRepo.Update(provider);
            }

            // 8. update Service AverageRating — same incremental formula
            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();
            var service = await serviceRepo.GetByIdAsync(request.ServiceId);

            if(service != null)
            {
                var reviewsCount = provider?.TotalReviews ?? 1;
                service.AverageRating += (dto.Rating - service.AverageRating) / (float)reviewsCount;

                serviceRepo.Update(service);
            }

            await _unitOfWork.SaveChangesAsync();

            var notificationMessage = new NotificationContentDto
            {
                UserId = request.Provider.UserId,
                Email = request.Provider.User?.Email,
                Subject = "تقييم جديد لخدمتك! ⭐️",
                Body = $"قام العميل بإضافة تقييم {dto.Rating} نجوم لخدمتك '{request.Service.Title}'. \nالتعليق: {dto.Comment ?? "بدون تعليق"}",
                ReferenceId = review.Id
            };

            await _notificationService.SendNotificationAsync(
                notificationMessage,
                NotificationType.Push,
                NotificationType.Email
            );

            review.ServiceRequest = request;
            review.Provider = request.Provider;
            review.Client = request.Client;

            return _mapper.Map<ReviewDto>(review);

        }

        public async Task<ReviewDto> UpdateReviewAsync(string userId, Guid reviewId, UpdateReviewDto dto)
        {
            var clientDto = await _clientService.GetMyProfileAsync(userId);

            if (dto.Rating is < 1 or > 5)
                throw new ReviewBadRequestException("Rating must be between 1 and 5");

            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            var reviewSpec = new ReviewByIdSpec(reviewId);
            var review = await reviewRepo.GetByIdWithSpecAsync(reviewSpec);

            if (review == null)
                throw new ReviewNotFoundException("Review not found");

            if (review.ClientId != clientDto.Id)
                throw new ReviewUnauthorizedException();

            var oldRating = review.Rating;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim();

            reviewRepo.Update(review);

            var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();
            var provider = await providerRepo.GetByIdAsync(review.ProviderId);

            if (provider != null && provider.TotalReviews > 0)
            {
                provider.AverageRating += (dto.Rating - oldRating) / (float)provider.TotalReviews;

                providerRepo.Update(provider);
            }

            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();
            var service = await serviceRepo.GetByIdAsync(review.ServiceId);

            if (service != null && provider?.TotalReviews > 0)
            {
                service.AverageRating +=
                    (dto.Rating - oldRating) / (float)provider.TotalReviews;

                serviceRepo.Update(service);
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReviewDto>(review);

        }
        public async Task DeleteReviewAsync(string userId, Guid reviewId)
        {
            var clientDto = await _clientService.GetMyProfileAsync(userId);

            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            var reviewSpec = new ReviewByIdSpec(reviewId);
            var review = await reviewRepo.GetByIdWithSpecAsync(reviewSpec);

            if (review == null)
                throw new ReviewNotFoundException("Review not found");

            if (review.ClientId != clientDto.Id)
                throw new ReviewUnauthorizedException();

            var deletedRating = review.Rating;

            reviewRepo.Delete(review);

            var providerRepo = _unitOfWork.GetRepository<SProvider, Guid>();
            var provider = await providerRepo.GetByIdAsync(review.ProviderId);

            if (provider != null && provider.TotalReviews > 0)
            {
                provider.TotalReviews--;

                if (provider.TotalReviews == 0)
                    provider.AverageRating = 0f;
                else
                {
                    provider.AverageRating =
                        (provider.AverageRating * (provider.TotalReviews + 1)
                                - deletedRating)
                                                 / provider.TotalReviews;
                }
                providerRepo.Update(provider);
            }

            var serviceRepo = _unitOfWork.GetRepository<Service, Guid>();
            var service = await serviceRepo.GetByIdAsync(review.ServiceId);

            if (service != null)
            {
                var reviewCount = provider?.TotalReviews ?? 0;

                if (reviewCount == 0)
                {
                    service.AverageRating = 0f;
                }
                else
                {
                    service.AverageRating =
                        (service.AverageRating * (reviewCount + 1)
                         - deletedRating)
                        / reviewCount;
                }

                serviceRepo.Update(service);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<ReviewDto> GetReviewByIdAsync(Guid reviewId)
        {
            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            var spec = new ReviewByIdSpec(reviewId);
            var review = await reviewRepo.GetByIdWithSpecAsync(spec);

            if (review == null)
                throw new ReviewNotFoundException("Review not found");

            return _mapper.Map<ReviewDto>(review);
        }
        public async Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(string userId)
        {
            var clientDto = await _clientService.GetMyProfileAsync(userId);
            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            var spec = new ReviewsByClientIdSpec(clientDto.Id);
            var reviews = await reviewRepo.GetAllWithSpecAsync(spec);

            return _mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
        }
        public async Task<IReadOnlyList<ReviewDto>> GetReviewsByServiceAsync(Guid serviceId)
        {
            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            var spec = new ReviewsByServiceIdSpec(serviceId);
            var reviews = await reviewRepo.GetAllWithSpecAsync(spec);

            return _mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
        }
        public async Task<IReadOnlyList<ReviewDto>> GetReviewsByProviderAsync(Guid providerId)
        {
            var reviewRepo = _unitOfWork.GetRepository<Review, Guid>();
            var spec = new ReviewsByProviderIdSpec(providerId);
            var reviews = await reviewRepo.GetAllWithSpecAsync(spec);

            return _mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
        }
    }
}

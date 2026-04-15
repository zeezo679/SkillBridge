using AutoMapper;
using E_commerce.Shared.Common.Dto.Review;
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

        public Task<ReviewDto> AddReviewAsync(string userId, AddReviewDto dto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteReviewAsync(string userId, Guid reviewId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ReviewDto> GetReviewByIdAsync(Guid reviewId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ReviewDto>> GetReviewsByServiceAsync(Guid serviceId)
        {
            throw new NotImplementedException();
        }

        public Task<ReviewDto> UpdateReviewAsync(string userId, Guid reviewId, UpdateReviewDto dto)
        {
            throw new NotImplementedException();
        }
    }
}

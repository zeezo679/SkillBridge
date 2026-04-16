using AutoMapper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Dto.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.AutoMapper.ReviewProfile
{
    public class ReviewMappingProfile: Profile
    {
        public ReviewMappingProfile()
        {
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.User.FullName))
                .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => src.Provider.User.FullName))
                .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.ServiceRequest.Service.Title));
        }
    }
}

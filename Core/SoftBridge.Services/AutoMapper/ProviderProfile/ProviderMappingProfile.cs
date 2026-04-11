using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Services.Resolver;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftBridge.Services.AutoMapper.ProviderProfile
{
    public class ProviderMappingProfile: Profile
    {
        public ProviderMappingProfile()
        {
            CreateMap<SProvider, ProviderProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom<PictureUrlResolver<SProvider, ProviderProfileDto>, string>(src => src.ProfileImageUrl))
                .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.Status.ToString()));


            CreateMap<Service, ServiceWithProviderDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images
                            .OrderBy(i => i.DisplayOrder)
                            .Select(i => i.ImageUrl)
                            .ToList()));


            CreateMap<ServiceRequest, IncomingRequestDto>()
            .ForMember(dest => dest.ServiceTitle,
                       opt => opt.MapFrom(src => src.Service.Title))
            .ForMember(dest => dest.ClientName, 
                       opt => opt.MapFrom(src => src.Client.User.FullName))
            .ForMember(dest => dest.Status,     
                       opt => opt.MapFrom(src => src.Status.ToString()));


            CreateMap<Review, ReceivedReviewDto>()
            .ForMember(dest => dest.ClientName,
                       opt => opt.MapFrom(src => src.Client.User.FullName))
            .ForMember(dest => dest.ServiceTitle,
                       opt => opt.MapFrom(src => src.ServiceRequest.Service.Title));



        }
    }
}

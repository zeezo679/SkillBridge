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
        }
    }
}

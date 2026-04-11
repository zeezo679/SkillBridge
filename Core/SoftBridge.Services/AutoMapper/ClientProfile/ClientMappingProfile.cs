using AutoMapper;
using SoftBridge.Shared.Common.Dto.ServiceRequest;

using SoftBridge.Domain.Models.AccountAggregates;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Dto.Client;
using System;
using System.Collections.Generic;
using System.Text;
using SoftBridge.Services.Resolver;

namespace SoftBridge.Services.AutoMapper.ClientProfile
{
    public class ClientMappingProfile: Profile
    {
        public ClientMappingProfile()
        {
            CreateMap<Client, ClientProfileDto>()
            .ForMember(dest => dest.Name,
                        opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email,
                        opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.ProfileImageUrl,
               opt => opt.MapFrom<PictureUrlResolver<Client, ClientProfileDto>, string>(src => src.ProfileImageUrl))
            .ForMember(dest => dest.CreatedAt,
                           opt => opt.MapFrom(src => src.CreatedAt));


            CreateMap<UpdateClientProfileDto, Client>();

        }
    }
}

using AutoMapper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Dto.ServiceRequest;
using SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos;

namespace SoftBridge.Services.AutoMapper.RequestProfile
{
    public class RequestMappingProfile: Profile
    {
        public RequestMappingProfile()
        {
            CreateMap<ServiceRequest, RequestDto>()
                .ForMember(dest => dest.ServiceTitle,
                       opt => opt.MapFrom(src => src.Service.Title))
                .ForMember(dest => dest.ClientName,
                           opt => opt.MapFrom(src => src.Client.User.FullName))
                .ForMember(dest => dest.ProviderName,
                           opt => opt.MapFrom(src => src.Provider.User.FullName))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()));



            CreateMap<ServiceRequest, RequestDetailsDto>()
                .ForMember(d => d.ServiceTitle,
                           o => o.MapFrom(s => s.Service.Title))
                .ForMember(d => d.ServiceCategory,
                           o => o.MapFrom(s => s.Service.Category.Name))
                .ForMember(d => d.ServicePrice,
                           o => o.MapFrom(s => s.Service.Price))
                .ForMember(d => d.ClientName,
                           o => o.MapFrom(s => s.Client.User.FullName))
                .ForMember(d => d.ClientEmail,
                           o => o.MapFrom(s => s.Client.User.Email))
                .ForMember(d => d.ProviderName,
                           o => o.MapFrom(s => s.Provider.User.FullName))
                .ForMember(d => d.ProviderEmail,
                           o => o.MapFrom(s => s.Provider.User.Email))
                .ForMember(d => d.Status,
                           o => o.MapFrom(s => s.Status.ToString()));


            CreateMap<Review, ReviewSummaryDto>();

        }
    }
}

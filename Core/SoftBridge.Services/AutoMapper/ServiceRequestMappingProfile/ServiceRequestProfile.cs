using AutoMapper;
using SoftBridge.Domain.Models.OrderAggregates;
using SoftBridge.Shared.Common.Dto.Review;
using SoftBridge.Shared.Common.Dto.ServiceRequest;


namespace SoftBridge.Services.AutoMapper.ServiceRequestMappingProfile
{
    public class ServiceRequestProfile: Profile
    {
        public ServiceRequestProfile()
        {
            CreateMap<ServiceRequest, ServiceRequestDto>()
            .ForMember(dest => dest.ServiceTitle,
                       opt => opt.MapFrom(src => src.Service.Title))
            .ForMember(dest => dest.ProviderName,
                       opt => opt.MapFrom(src => src.Provider.User.FullName))
            .ForMember(dest => dest.Status,
                       opt => opt.MapFrom(src => src.Status.ToString()));


            CreateMap<Review, ReviewSummaryDto>();

            CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.ServiceTitle,
                       opt => opt.MapFrom(src => src.ServiceRequest.Service.Title))
            .ForMember(dest => dest.ProviderName,
                       opt => opt.MapFrom(src => src.Provider.User.FullName));
        }
    }
}

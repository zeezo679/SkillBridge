using AutoMapper;
using E_commerce.Shared.Common.Dto.Service;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Services.Resolver;
using SoftBridge.Shared.Common.Dto.Service;
using System;

namespace SoftBridge.Services.AutoMapper.ServiceProfile;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        // From DTO to Domain (Creation) Ignore the Images because we will handle them separately in the service layer
        // BY the Attachment service and we will just save the URLs in the ServiceImage entity
        CreateMap<CreateServiceDto, Service>()
                 .ForMember(dest => dest.Images, opt => opt.Ignore());

        // Image mapping
        CreateMap<ServiceImage, ServiceImageDto>()
                .ForMember(dest => dest.ImageUrl, opt =>
                    opt.MapFrom<PictureUrlResolver<ServiceImage, ServiceImageDto>, string>(src => src.ImageUrl));

        // From Domain to DTO (For Display)
        // the auto mapper will automatically map the properties with the same name and type,
        // but for the Status property we need to convert it to string because it's an enum in the domain model and a string in the DTO\
        // and the automapper is intelligence to map the Serviceimage collection to the ServiceImageDto collection using the mapping we defined above
        CreateMap<Service, ServiceDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));


    }
}

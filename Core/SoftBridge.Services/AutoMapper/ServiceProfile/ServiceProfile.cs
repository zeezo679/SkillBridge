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
        CreateMap<CreateServiceDto, Service>()
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // 1 - mapping images
        CreateMap<ServiceImage, ServiceImageDto>()
                .ForMember(dest => dest.ImageUrl, opt =>
                    opt.MapFrom<PictureUrlResolver<ServiceImage, ServiceImageDto>, string>(src => src.ImageUrl));

        // 2. (ServiceDto)
        CreateMap<Service, ServiceDto>()
            .ForMember(dest => dest.ThumbnailUrl, opt =>
                opt.MapFrom<PictureUrlResolver<Service, ServiceDto>, string>(
                    // we need to send the protofolio image only 
                    src => src.Images.FirstOrDefault(i => !i.IsPortfolio) != null
                         ? src.Images.FirstOrDefault(i => !i.IsPortfolio)!.ImageUrl
                         : string.Empty));

        // 3. Details mapping 
        CreateMap<Service, ServiceDetailsDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => src.Provider.User.FullName ?? string.Empty));
        // the images are mapping from the first CreateMapping in this file

        CreateMap<UpdateServiceDto, Service>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) 
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore()) 
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());
    }
}

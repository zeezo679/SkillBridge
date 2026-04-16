using System;
using AutoMapper;
using SoftBridge.Shared.Common.Dto.Category;
using SoftBridge.Domain.Models.ServiceAggregates;

namespace SoftBridge.Services.AutoMapper.CategoryProfile;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>();

        CreateMap<Category, CategoryWithServicesDto>();

        CreateMap<CategoryToCreateDto, Category>();
        
        CreateMap<CategoryToUpdateDto, Category>();

    }
}

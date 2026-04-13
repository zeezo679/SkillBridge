using AutoMapper;
using SoftBridge.Abstraction.IServices.Category;
using SoftBridge.Domain.Contracts.UnitOfWorkPattern;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Shared.Common.Dto.Category;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Category;
using SoftBridge.Services.Specification.CategorySpecifications.GetAllCategoriesAsync;
using SoftBridge.Services.Specification.CategorySpecifications.GetCategoryByIdAsync;
using SoftBridge.Domain.Exceptions.NotFoundModels.Category;
using SoftBridge.Domain.Exceptions;

namespace SoftBridge.Services.Services.CategoryImplementation;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginationResponse<CategoryDto>> GetAllCategoriesAsync(CategoryQueryParams parameters)
    {
        var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

        var spec = new CategoryFiltrationSpecification(parameters);
        var categories = await categoryRepo.GetAllWithSpecAsync(spec);

        var countSpec = new CategoryCountSpecification(parameters);
        var totalItems = await categoryRepo.GetCountAsync(countSpec);

        return new PaginationResponse<CategoryDto>(parameters.PageIndex, parameters.PageSize, totalItems, _mapper.Map<IReadOnlyList<CategoryDto>>(categories));
    }

    public async Task<CategoryWithServicesDto> GetCategoryByIdAsync(Guid id)
    {
        var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

        var spec = new CategoryIncludesServicesSpecification(id);
        var category = await categoryRepo.GetByIdWithSpecAsync(spec);

        if (category is null)
            throw new CategoryNotFoundException("Category not found");

        return _mapper.Map<Category, CategoryWithServicesDto>(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryToCreateDto categoryDto)
    {
        var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

        var category = _mapper.Map<CategoryToCreateDto, Category>(categoryDto);

        category.Id = Guid.NewGuid();

        await categoryRepo.AddAsync(category);

        var result = await _unitOfWork.SaveChangesAsync();

        if (result <= 0)
            throw new BadRequestExceptionCustome("Failed to create category");

        return _mapper.Map<Category, CategoryDto>(category);
    }


    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, CategoryToUpdateDto categoryToUpdateDto)
    {
        var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

        var category = await categoryRepo.GetByIdAsync(id);

        if (category is null)
            throw new CategoryNotFoundException("Category not found");

        //map manually
        category.Name = categoryToUpdateDto.Name;
        category.IconUrl = categoryToUpdateDto.IconUrl;
        category.IsActive = categoryToUpdateDto.IsActive;

        categoryRepo.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<Category, CategoryDto>(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

        var spec = new CategoryIncludesServicesSpecification(id);
        var category = await categoryRepo.GetByIdWithSpecAsync(spec);

        if (category is null)
            throw new CategoryNotFoundException("Category not found");

        if (category.Services.Any())
            throw new BadRequestExceptionCustome("Cannot delete category with existing services. Reassign or delete them first.");    

        categoryRepo.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

}

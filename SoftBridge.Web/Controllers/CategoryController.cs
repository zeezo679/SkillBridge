using System;
using Microsoft.AspNetCore.Mvc;
using SoftBridge.Abstraction.IServices.Category;
using SoftBridge.Domain.Models.ServiceAggregates;
using SoftBridge.Shared.Common.Dto.Category;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Category;

namespace SoftBridge.Web.Controllers;


public class CategoryController(ICategoryService categoryService) : AppBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParams queryParams)
    {
        var result = await categoryService.GetAllCategoriesAsync(queryParams);
        return Success<PaginationResponse<CategoryDto>>(result, "Categories retrieved successfully");
    }

    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await categoryService.GetCategoryByIdAsync(id);

        return Success<CategoryWithServicesDto>(result, "Category retrieved successfully");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryToCreateDto categoryDto)
    {
        var result = await categoryService.CreateCategoryAsync(categoryDto);
        return Created<CategoryDto>(result, "Category created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryToUpdateDto categoryDto)
    {
        var result = await categoryService.UpdateCategoryAsync(id, categoryDto);
        return Success<CategoryDto>(result, "Category updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await categoryService.DeleteCategoryAsync(id);
        return Success("Category deleted successfully");
    }
}

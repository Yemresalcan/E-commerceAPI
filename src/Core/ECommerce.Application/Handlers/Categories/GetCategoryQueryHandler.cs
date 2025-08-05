using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Categories;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Categories;

/// <summary>
/// Handler for GetCategoryQuery
/// </summary>
public class GetCategoryQueryHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ILogger<GetCategoryQueryHandler> logger) : IQueryHandler<GetCategoryQuery, CategoryDto?>
{
    public async Task<CategoryDto?> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting category with ID: {CategoryId}", request.CategoryId);

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        
        if (category == null)
        {
            logger.LogWarning("Category not found with ID: {CategoryId}", request.CategoryId);
            return null;
        }

        var result = mapper.Map<CategoryDto>(category);
        
        logger.LogInformation("Retrieved category: {CategoryName}", category.Name);
        return result;
    }
}
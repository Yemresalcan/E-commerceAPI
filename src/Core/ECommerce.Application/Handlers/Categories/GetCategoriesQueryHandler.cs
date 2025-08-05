using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Categories;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Handlers.Categories;

/// <summary>
/// Handler for GetCategoriesQuery
/// </summary>
public class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ILogger<GetCategoriesQueryHandler> logger) : IQueryHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting categories with ParentId: {ParentId}, IncludeInactive: {IncludeInactive}", 
            request.ParentId, request.IncludeInactive);

        IEnumerable<Domain.Aggregates.ProductAggregate.Category> categories;

        if (request.ParentId.HasValue)
        {
            categories = await categoryRepository.GetByParentIdAsync(request.ParentId.Value, request.IncludeInactive, cancellationToken);
        }
        else
        {
            categories = await categoryRepository.GetRootCategoriesAsync(request.IncludeInactive, cancellationToken);
        }

        var result = mapper.Map<IEnumerable<CategoryDto>>(categories);
        
        logger.LogInformation("Retrieved {Count} categories", result.Count());
        return result;
    }
}
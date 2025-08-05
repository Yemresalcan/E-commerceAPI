using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Queries.Categories;

/// <summary>
/// Query to get categories with optional filtering
/// </summary>
public record GetCategoriesQuery(
    bool IncludeInactive = false,
    Guid? ParentId = null
) : IQuery<IEnumerable<CategoryDto>>;
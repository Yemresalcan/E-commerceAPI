using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Queries.Categories;

/// <summary>
/// Query to get a specific category by ID
/// </summary>
public record GetCategoryQuery(
    Guid CategoryId
) : IQuery<CategoryDto?>;
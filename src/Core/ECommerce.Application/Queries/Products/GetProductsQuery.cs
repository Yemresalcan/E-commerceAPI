using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Queries.Products;

/// <summary>
/// Query to get products with pagination and filtering
/// </summary>
public record GetProductsQuery(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStockOnly = null,
    bool? FeaturedOnly = null,
    List<string>? Tags = null,
    decimal? MinRating = null,
    string? SortBy = "relevance",
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Queries.Products;

/// <summary>
/// Query to search products using Elasticsearch with advanced features
/// </summary>
public record SearchProductsQuery(
    string Query,
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
) : IRequest<ProductSearchResultDto>;

/// <summary>
/// Data transfer object for product search results with facets
/// </summary>
public record ProductSearchResultDto(
    IEnumerable<ProductDto> Products,
    long TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    ProductSearchFacetsDto Facets
);

/// <summary>
/// Data transfer object for product search facets
/// </summary>
public record ProductSearchFacetsDto(
    Dictionary<string, long> Categories,
    Dictionary<string, long> PriceRanges,
    Dictionary<string, long> Brands,
    long InStockCount,
    double AverageRating
);
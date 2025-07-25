using ECommerce.ReadModel.Models;
using Nest;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Interface for product search operations
/// </summary>
public interface IProductSearchService : IElasticsearchService<ProductReadModel>
{
    /// <summary>
    /// Searches products with advanced filtering and faceting
    /// </summary>
    Task<ProductSearchResult> SearchProductsAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product suggestions for autocomplete
    /// </summary>
    Task<IEnumerable<string>> GetProductSuggestionsAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by category
    /// </summary>
    Task<ISearchResponse<ProductReadModel>> SearchByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets featured products
    /// </summary>
    Task<ISearchResponse<ProductReadModel>> GetFeaturedProductsAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products with low stock
    /// </summary>
    Task<ISearchResponse<ProductReadModel>> GetLowStockProductsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets similar products based on category and tags
    /// </summary>
    Task<ISearchResponse<ProductReadModel>> GetSimilarProductsAsync(Guid productId, int count = 5, CancellationToken cancellationToken = default);
}

/// <summary>
/// Product search request parameters
/// </summary>
public class ProductSearchRequest
{
    public string? Query { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public bool? FeaturedOnly { get; set; }
    public List<string>? Tags { get; set; }
    public decimal? MinRating { get; set; }
    public string? SortBy { get; set; } = "relevance"; // relevance, price_asc, price_desc, rating, newest
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Product search result with facets and aggregations
/// </summary>
public class ProductSearchResult
{
    public IEnumerable<ProductReadModel> Products { get; set; } = [];
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public ProductSearchFacets Facets { get; set; } = new();
}

/// <summary>
/// Search facets for product filtering
/// </summary>
public class ProductSearchFacets
{
    public Dictionary<string, long> Categories { get; set; } = new();
    public Dictionary<string, long> PriceRanges { get; set; } = new();
    public Dictionary<string, long> Brands { get; set; } = new();
    public long InStockCount { get; set; }
    public double AverageRating { get; set; }
}
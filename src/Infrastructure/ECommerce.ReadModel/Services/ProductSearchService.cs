using ECommerce.ReadModel.Configuration;
using ECommerce.ReadModel.Configurations;
using ECommerce.ReadModel.Models;
using Microsoft.Extensions.Options;
using Nest;
using ECommerce.Application.Interfaces;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Implementation of product search operations using Elasticsearch
/// </summary>
public class ProductSearchService : BaseElasticsearchService<ProductReadModel>, IProductSearchService
{
    protected override string IndexName => ProductIndexConfiguration.IndexName;

    public ProductSearchService(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> settings,
        ILogger<ProductSearchService> logger,
        ICacheService? cacheService = null)
        : base(elasticClient, settings, logger, cacheService)
    {
    }

    public override async Task<bool> CreateIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var indexName = GetIndexName();
            var indexMapping = ProductIndexConfiguration.GetIndexMapping(indexName);

            var response = await _elasticClient.Indices.CreateAsync(indexMapping, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to create product index {IndexName}: {Error}", indexName, response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogInformation("Successfully created product index: {IndexName}", indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating product index");
            return false;
        }
    }

    public override async Task<ISearchResponse<ProductReadModel>> SimpleSearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<ProductReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields(f => f
                                .Field(p => p.Name, 3.0) // Increased boost for name
                                .Field(p => p.Name.Suffix("autocomplete"), 2.0) // Boost autocomplete
                                .Field(p => p.Description, 1.0)
                                .Field(p => p.Sku, 2.5) // Increased boost for SKU
                                .Field(p => p.Category.Name, 1.5)
                                .Field(p => p.Tags, 1.2) // Added tags field
                            )
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(Fuzziness.Auto)
                            .MinimumShouldMatch("75%") // Improved relevance
                        )
                    )
                    .Filter(f => f
                        .Term(t => t.IsActive, true)
                    )
                    .Should(s => s // Boost popular products
                        .Range(r => r
                            .Field(p => p.AverageRating)
                            .GreaterThanOrEquals(4.0)
                        )
                    )
                )
            )
            .Sort(s => s
                .Descending(SortSpecialField.Score)
                .Descending(p => p.IsFeatured)
                .Descending(p => p.AverageRating)
                .Descending(p => p.ReviewCount) // Added review count for better sorting
            )
            .Highlight(h => h
                .Fields(f => f
                    .Field(p => p.Name)
                    .Field(p => p.Description)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
                .FragmentSize(150) // Optimized fragment size
                .NumberOfFragments(2)
            )
            .Source(s => s.Excludes(e => e.Field(p => p.Description))) // Exclude large fields for performance
            .TrackTotalHits(true); // Enable accurate total count

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public override async Task<ISearchResponse<ProductReadModel>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<ProductReadModel>()
            .Index(GetIndexName())
            .Size(10)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name.Suffix("autocomplete"))
                    .Query(query)
                )
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ProductSearchResult> SearchProductsAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<ProductReadModel>()
            .Index(GetIndexName())
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize)
            .Query(q => BuildProductQuery(q, request))
            .Sort(s => BuildProductSort(s, request.SortBy))
            .Aggregations(a => ProductIndexConfiguration.GetAggregations())
            .Highlight(h => h
                .Fields(f => f
                    .Field(p => p.Name)
                    .Field(p => p.Description)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
            )
            .Source(s => s.Excludes(e => e.Field(p => p.Description))) // Exclude large fields for performance
            .TrackTotalHits(true);

        // Generate cache key for this search with performance optimization
        var cacheKey = GenerateSearchCacheKey(request);
        var cacheDuration = GetCacheDurationForSearch(request);

        var response = await CachedSearchAsync(searchDescriptor, cacheKey, cacheDuration, cancellationToken);

        return new ProductSearchResult
        {
            Products = response.Documents,
            TotalCount = response.Total,
            Page = request.Page,
            PageSize = request.PageSize,
            Facets = ExtractProductFacets(response.Aggregations)
        };
    }

    public async Task<IEnumerable<string>> GetProductSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchResponse = await GetSuggestionsAsync(query, cancellationToken);
        
        return searchResponse.Documents
            .Select(p => p.Name)
            .Distinct()
            .Take(10);
    }

    public async Task<ISearchResponse<ProductReadModel>> SearchByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<ProductReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .Term(t => t.Category.Id, categoryId) &&
                        f.Term(t => t.IsActive, true)
                    )
                )
            )
            .Sort(s => s
                .Descending(p => p.IsFeatured)
                .Descending(p => p.AverageRating)
                .Descending(p => p.CreatedAt)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<ProductReadModel>> GetFeaturedProductsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<ProductReadModel>()
            .Index(GetIndexName())
            .Size(count)
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .Term(t => t.IsFeatured, true) &&
                        f.Term(t => t.IsActive, true) &&
                        f.Term(t => t.IsInStock, true)
                    )
                )
            )
            .Sort(s => s
                .Descending(p => p.AverageRating)
                .Descending(p => p.ReviewCount)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<ProductReadModel>> GetLowStockProductsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<ProductReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .Term(t => t.IsLowStock, true) &&
                        f.Term(t => t.IsActive, true)
                    )
                )
            )
            .Sort(s => s
                .Ascending(p => p.StockQuantity)
                .Descending(p => p.UpdatedAt)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<ProductReadModel>> GetSimilarProductsAsync(Guid productId, int count = 5, CancellationToken cancellationToken = default)
    {
        // First get the product to find similar ones
        var product = await GetDocumentAsync(productId, cancellationToken);
        if (product == null)
        {
            return new SearchResponse<ProductReadModel>();
        }

        var searchDescriptor = new SearchDescriptor<ProductReadModel>()
            .Index(GetIndexName())
            .Size(count)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        s => s.Term(t => t.Category.Id, product.Category.Id),
                        s => s.Terms(t => t.Field(f => f.Tags).Terms(product.Tags))
                    )
                    .Filter(f => f
                        .Bool(bf => bf
                            .Must(m => m.Term(t => t.IsActive, true))
                            .MustNot(mn => mn.Term(t => t.Id, productId))
                        )
                    )
                    .MinimumShouldMatch(1)
                )
            )
            .Sort(s => s
                .Descending(SortSpecialField.Score)
                .Descending(p => p.AverageRating)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    private QueryContainer BuildProductQuery(QueryContainerDescriptor<ProductReadModel> q, ProductSearchRequest request)
    {
        var queries = new List<QueryContainer>();

        // Text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queries.Add(q.MultiMatch(mm => mm
                .Query(request.Query)
                .Fields(f => f
                    .Field(p => p.Name, 2.0)
                    .Field(p => p.Description)
                    .Field(p => p.Sku, 1.5)
                    .Field(p => p.Category.Name)
                )
                .Type(TextQueryType.BestFields)
                .Fuzziness(Fuzziness.Auto)
            ));
        }

        var filters = new List<QueryContainer>
        {
            q.Term(t => t.IsActive, true)
        };

        // Category filter
        if (request.CategoryId.HasValue)
        {
            filters.Add(q.Term(t => t.Category.Id, request.CategoryId.Value));
        }

        // Price range filter
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            filters.Add(q.Range(r => r
                .Field(f => f.Price)
                .GreaterThanOrEquals((double?)request.MinPrice)
                .LessThanOrEquals((double?)request.MaxPrice)
            ));
        }

        // Stock filter
        if (request.InStockOnly == true)
        {
            filters.Add(q.Term(t => t.IsInStock, true));
        }

        // Featured filter
        if (request.FeaturedOnly == true)
        {
            filters.Add(q.Term(t => t.IsFeatured, true));
        }

        // Tags filter
        if (request.Tags?.Any() == true)
        {
            filters.Add(q.Terms(t => t.Field(f => f.Tags).Terms(request.Tags)));
        }

        // Rating filter
        if (request.MinRating.HasValue)
        {
            filters.Add(q.Range(r => r
                .Field(f => f.AverageRating)
                .GreaterThanOrEquals((double)request.MinRating.Value)
            ));
        }

        return q.Bool(b => b
            .Must(queries.ToArray())
            .Filter(filters.ToArray())
        );
    }

    private SortDescriptor<ProductReadModel> BuildProductSort(SortDescriptor<ProductReadModel> s, string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "price_asc" => s.Ascending(p => p.Price),
            "price_desc" => s.Descending(p => p.Price),
            "rating" => s.Descending(p => p.AverageRating).Descending(p => p.ReviewCount),
            "newest" => s.Descending(p => p.CreatedAt),
            _ => s.Descending(SortSpecialField.Score).Descending(p => p.IsFeatured).Descending(p => p.AverageRating)
        };
    }

    private ProductSearchFacets ExtractProductFacets(AggregateDictionary aggregations)
    {
        var facets = new ProductSearchFacets();

        if (aggregations.Terms("categories") is { } categoriesAgg)
        {
            facets.Categories = categoriesAgg.Buckets.ToDictionary(b => b.Key, b => b.DocCount ?? 0);
        }

        if (aggregations.Range("price_ranges") is { } priceRangesAgg)
        {
            facets.PriceRanges = priceRangesAgg.Buckets.ToDictionary(b => b.Key, b => b.DocCount);        }

        if (aggregations.Terms("brands") is { } brandsAgg)
        {
            facets.Brands = brandsAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0);
        }

        if (aggregations.Filter("in_stock") is { } inStockAgg)
        {
            facets.InStockCount = inStockAgg.DocCount;
        }

        if (aggregations.Average("avg_rating") is { } avgRatingAgg)
        {
            facets.AverageRating = avgRatingAgg.Value ?? 0;
        }

        return facets;
    }

    /// <summary>
    /// Generates a cache key for product search requests
    /// </summary>
    private string GenerateSearchCacheKey(ProductSearchRequest request)
    {
        var keyParts = new List<string>
        {
            "product_search",
            request.Query ?? "all",
            request.Page.ToString(),
            request.PageSize.ToString(),
            request.SortBy ?? "relevance"
        };

        if (request.CategoryId.HasValue)
            keyParts.Add($"cat_{request.CategoryId}");

        if (request.MinPrice.HasValue)
            keyParts.Add($"minp_{request.MinPrice}");

        if (request.MaxPrice.HasValue)
            keyParts.Add($"maxp_{request.MaxPrice}");

        if (request.InStockOnly == true)
            keyParts.Add("instock");

        if (request.FeaturedOnly == true)
            keyParts.Add("featured");

        if (request.MinRating.HasValue)
            keyParts.Add($"rating_{request.MinRating}");

        if (request.Tags?.Any() == true)
            keyParts.Add($"tags_{string.Join("_", request.Tags.OrderBy(t => t))}");

        return string.Join(":", keyParts).ToLowerInvariant();
    }

    /// <summary>
    /// Gets optimized cache duration based on search request characteristics
    /// </summary>
    private TimeSpan GetCacheDurationForSearch(ProductSearchRequest request)
    {
        // Popular searches (no filters) - cache longer
        if (string.IsNullOrEmpty(request.Query) && !request.CategoryId.HasValue && 
            !request.MinPrice.HasValue && !request.MaxPrice.HasValue)
        {
            return TimeSpan.FromMinutes(30);
        }

        // Category-based searches - cache moderately
        if (request.CategoryId.HasValue && string.IsNullOrEmpty(request.Query))
        {
            return TimeSpan.FromMinutes(20);
        }

        // Text searches - cache shorter due to personalization
        if (!string.IsNullOrEmpty(request.Query))
        {
            return TimeSpan.FromMinutes(10);
        }

        // Complex filtered searches - cache shortest
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue || 
            request.Tags?.Any() == true || request.MinRating.HasValue)
        {
            return TimeSpan.FromMinutes(5);
        }

        // Default cache duration
        return TimeSpan.FromMinutes(15);
    }

    /// <summary>
    /// Optimized search with request preprocessing
    /// </summary>
    private SearchDescriptor<ProductReadModel> OptimizeSearchDescriptor(SearchDescriptor<ProductReadModel> descriptor, ProductSearchRequest request)
    {
        // Add request timeout for performance
        descriptor.Timeout("30s");

        // Optimize source filtering - exclude large fields by default
        descriptor.Source(s => s.Excludes(e => e.Field(p => p.Description)));

        // Add preference for consistent shard routing
        if (!string.IsNullOrEmpty(request.Query))
        {
            descriptor.Preference($"search_{request.Query.GetHashCode()}");
        }

        // Enable query cache for repeated queries
        descriptor.RequestCache(true);

        // Optimize batch size for large result sets
        if (request.PageSize > 50)
        {
            descriptor.BatchedReduceSize(512);
        }

        return descriptor;
    }

    /// <summary>
    /// Pre-processes search request for better performance
    /// </summary>
    private ProductSearchRequest PreprocessSearchRequest(ProductSearchRequest request)
    {
        // Normalize and optimize search query
        if (!string.IsNullOrEmpty(request.Query))
        {
            request.Query = request.Query.Trim().ToLowerInvariant();
            
            // Remove common stop words that don't add value
            var stopWords = new[] { "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
            var words = request.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var filteredWords = words.Where(w => !stopWords.Contains(w) && w.Length > 1);
            request.Query = string.Join(" ", filteredWords);
        }

        // Optimize page size
        if (request.PageSize > 100)
        {
            request.PageSize = 100; // Limit max page size for performance
        }

        // Optimize price ranges
        if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
        {
            (request.MinPrice, request.MaxPrice) = (request.MaxPrice, request.MinPrice);
        }

        return request;
    }
}
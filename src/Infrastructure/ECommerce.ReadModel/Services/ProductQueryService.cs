using AutoMapper;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Products;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Implementation of product query service using Elasticsearch
/// </summary>
public class ProductQueryService(
    IProductSearchService productSearchService,
    IMapper mapper,
    ILogger<ProductQueryService> logger)
    : IProductQueryService
{
    public async Task<PagedResult<ProductDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting products with search term: {SearchTerm}", query.SearchTerm);

        var searchRequest = new ProductSearchRequest
        {
            Query = query.SearchTerm,
            CategoryId = query.CategoryId,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            InStockOnly = query.InStockOnly,
            FeaturedOnly = query.FeaturedOnly,
            Tags = query.Tags,
            MinRating = query.MinRating,
            SortBy = query.SortBy,
            Page = query.Page,
            PageSize = query.PageSize
        };

        var searchResult = await productSearchService.SearchProductsAsync(searchRequest, cancellationToken);
        var productDtos = mapper.Map<IEnumerable<ProductDto>>(searchResult.Products);

        return new PagedResult<ProductDto>
        {
            Items = productDtos.ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = (int)searchResult.TotalCount
        };
    }

    public async Task<ProductSearchResultDto> SearchProductsAsync(SearchProductsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Searching products with query: {Query}", query.Query);

        var searchRequest = new ProductSearchRequest
        {
            Query = query.Query,
            CategoryId = query.CategoryId,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            InStockOnly = query.InStockOnly,
            FeaturedOnly = query.FeaturedOnly,
            Tags = query.Tags,
            MinRating = query.MinRating,
            SortBy = query.SortBy,
            Page = query.Page,
            PageSize = query.PageSize
        };

        var searchResult = await productSearchService.SearchProductsAsync(searchRequest, cancellationToken);
        var productDtos = mapper.Map<IEnumerable<ProductDto>>(searchResult.Products);
        var facetsDto = mapper.Map<ProductSearchFacetsDto>(searchResult.Facets);

        return new ProductSearchResultDto(
            productDtos,
            searchResult.TotalCount,
            searchResult.Page,
            searchResult.PageSize,
            searchResult.TotalPages,
            facetsDto
        );
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting product by ID: {ProductId}", productId);

        var product = await productSearchService.GetDocumentAsync(productId, cancellationToken);
        return product == null ? null : mapper.Map<ProductDto>(product);
    }
}
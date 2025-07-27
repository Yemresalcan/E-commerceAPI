using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.Products;
using MediatR;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Caching.Decorators;

/// <summary>
/// Caching decorator for GetProductsQuery handler
/// </summary>
public class CachedGetProductsQueryHandlerDecorator(
    IRequestHandler<GetProductsQuery, PagedResult<ProductDto>> handler,
    ICacheService cacheService,
    IOptions<CacheOptions> cacheOptions,
    ILogger<CachedGetProductsQueryHandlerDecorator> logger)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        if (!_cacheOptions.Enabled)
        {
            return await handler.Handle(request, cancellationToken);
        }

        var cacheKey = CacheKeyGenerator.ProductsList(request.Page, request.PageSize, request.SearchTerm, request.CategoryId);
        var expiration = TimeSpan.FromMinutes(_cacheOptions.ProductCacheExpirationMinutes);

        logger.LogDebug("Attempting to retrieve cached products list with key: {CacheKey}", cacheKey);

        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => handler.Handle(request, cancellationToken),
            expiration,
            cancellationToken);
    }
}

/// <summary>
/// Caching decorator for SearchProductsQuery handler
/// </summary>
public class CachedSearchProductsQueryHandlerDecorator(
    IRequestHandler<SearchProductsQuery, ProductSearchResultDto> handler,
    ICacheService cacheService,
    IOptions<CacheOptions> cacheOptions,
    ILogger<CachedSearchProductsQueryHandlerDecorator> logger)
    : IRequestHandler<SearchProductsQuery, ProductSearchResultDto>
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;

    public async Task<ProductSearchResultDto> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        if (!_cacheOptions.Enabled)
        {
            return await handler.Handle(request, cancellationToken);
        }

        var cacheKey = CacheKeyGenerator.ProductsSearch(request.Query, request.Page, request.PageSize);
        var expiration = TimeSpan.FromMinutes(_cacheOptions.SearchCacheExpirationMinutes);

        logger.LogDebug("Attempting to retrieve cached product search results with key: {CacheKey}", cacheKey);

        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => handler.Handle(request, cancellationToken),
            expiration,
            cancellationToken);
    }
}
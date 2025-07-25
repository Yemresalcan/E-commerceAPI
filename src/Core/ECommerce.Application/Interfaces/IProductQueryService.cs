using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs;
using ECommerce.Application.Queries.Products;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for product query operations
/// </summary>
public interface IProductQueryService
{
    /// <summary>
    /// Searches products with pagination and filtering
    /// </summary>
    Task<PagedResult<ProductDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products with advanced features and facets
    /// </summary>
    Task<ProductSearchResultDto> SearchProductsAsync(SearchProductsQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single product by ID
    /// </summary>
    Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
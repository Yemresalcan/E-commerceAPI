using ECommerce.Domain.Aggregates.ProductAggregate;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for Product aggregate operations
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Gets products by category identifier
    /// </summary>
    /// <param name="categoryId">The category identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products in the specified category</returns>
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products by SKU
    /// </summary>
    /// <param name="sku">The product SKU</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The product with the specified SKU, or null if not found</returns>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active products (products that are currently available)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active products</returns>
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets featured products
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of featured products</returns>
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products with low stock (stock quantity at or below minimum level)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products with low stock</returns>
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products that are out of stock
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of out-of-stock products</returns>
    Task<IEnumerable<Product>> GetOutOfStockProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by name or description
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products matching the search term</returns>
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products with pagination support
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of products</returns>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products by multiple identifiers
    /// </summary>
    /// <param name="productIds">Collection of product identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of products with the specified identifiers</returns>
    Task<IEnumerable<Product>> GetByIdsAsync(
        IEnumerable<Guid> productIds, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product with the specified SKU exists
    /// </summary>
    /// <param name="sku">The product SKU</param>
    /// <param name="excludeId">Optional product ID to exclude from the check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a product with the SKU exists, false otherwise</returns>
    Task<bool> ExistsBySkuAsync(
        string sku, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default);
}
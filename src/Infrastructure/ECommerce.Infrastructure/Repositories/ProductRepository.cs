using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation using Entity Framework Core
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ILogger<ProductRepository> _productLogger;

    public ProductRepository(ECommerceDbContext context, ILogger<ProductRepository> logger, ILogger<Repository<Product>> baseLogger) 
        : base(context, baseLogger)
    {
        _productLogger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets products by category identifier
    /// </summary>
    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets products by SKU
    /// </summary>
    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        using (_productLogger.BeginRepositoryScope("ProductRepository", "GetBySku"))
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                _productLogger.LogDebug("GetBySkuAsync called with null or empty SKU");
                return null;
            }

            _productLogger.LogDebug("Getting product by SKU: {Sku}", sku);
            
            var result = await _dbSet
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);

            if (result == null)
            {
                _productLogger.LogDebug("Product with SKU {Sku} not found", sku);
            }
            else
            {
                _productLogger.LogDebug("Successfully retrieved product {ProductId} with SKU: {Sku}", result.Id, sku);
            }

            return result;
        }
    }

    /// <summary>
    /// Gets active products (products that are currently available)
    /// </summary>
    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets featured products
    /// </summary>
    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsFeatured && p.IsActive)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets products with low stock (stock quantity at or below minimum level)
    /// </summary>
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= p.MinimumStockLevel && p.StockQuantity > 0)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets products that are out of stock
    /// </summary>
    public async Task<IEnumerable<Product>> GetOutOfStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= 0)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Searches products by name or description
    /// </summary>
    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return [];

        var lowerSearchTerm = searchTerm.ToLower();

        return await _dbSet
            .Where(p => p.IsActive && 
                       (p.Name.ToLower().Contains(lowerSearchTerm) || 
                        p.Description.ToLower().Contains(lowerSearchTerm)))
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets products with pagination support
    /// </summary>
    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalCount = await _dbSet.CountAsync(cancellationToken);
        
        var products = await _dbSet
            .Include(p => p.Reviews)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    /// <summary>
    /// Gets products by multiple identifiers
    /// </summary>
    public async Task<IEnumerable<Product>> GetByIdsAsync(
        IEnumerable<Guid> productIds, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(productIds);

        var ids = productIds.ToList();
        if (!ids.Any())
            return [];

        return await _dbSet
            .Where(p => ids.Contains(p.Id))
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a product with the specified SKU exists
    /// </summary>
    public async Task<bool> ExistsBySkuAsync(
        string sku, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        var query = _dbSet.Where(p => p.Sku == sku);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Override GetByIdAsync to include reviews
    /// </summary>
    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <summary>
    /// Override GetAllAsync to include reviews
    /// </summary>
    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);
    }
}
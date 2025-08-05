using ECommerce.Application.Interfaces;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Caching;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation using Entity Framework Core
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ILogger<ProductRepository> _productLogger;
    private readonly ICacheService? _cacheService;

    // Cache durations for different types of queries
    private static readonly TimeSpan ProductCacheDuration = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan ProductListCacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan FeaturedProductsCacheDuration = TimeSpan.FromMinutes(120);

    public ProductRepository(
        ECommerceDbContext context, 
        ILogger<ProductRepository> logger, 
        ILogger<Repository<Product>> baseLogger,
        ICacheService? cacheService = null) 
        : base(context, baseLogger)
    {
        _productLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService;
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
    /// Gets featured products with caching optimization
    /// </summary>
    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyGenerator.FeaturedProducts();
        
        if (_cacheService != null)
        {
            var cachedResult = await _cacheService.GetAsync<IEnumerable<Product>>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _productLogger.LogDebug("Cache hit for featured products");
                return cachedResult;
            }
        }

        var products = await _dbSet
            .Where(p => p.IsFeatured && p.IsActive)
            .Include(p => p.Reviews)
            .ToListAsync(cancellationToken);

        // Cache the result
        if (_cacheService != null && products.Any())
        {
            await _cacheService.SetAsync(cacheKey, products, FeaturedProductsCacheDuration, cancellationToken);
            _productLogger.LogDebug("Cached featured products for {Duration}", FeaturedProductsCacheDuration);
        }

        return products;
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
    public new async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
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
    public new async Task<IEnumerable<Product>> GetByIdsAsync(
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
    /// Override GetByIdAsync to get fresh data from database with proper tracking handling
    /// </summary>
    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _productLogger.LogDebug("Getting product by ID: {ProductId} (fresh from database)", id);
        
        // First check if entity is already being tracked
        var trackedEntity = _context.ChangeTracker.Entries<Product>()
            .FirstOrDefault(e => e.Entity.Id == id)?.Entity;
            
        if (trackedEntity != null)
        {
            _productLogger.LogDebug("Found tracked product: {ProductName} with stock: {StockQuantity}", 
                trackedEntity.Name, trackedEntity.StockQuantity);
            
            // Reload the entity to get fresh data from database
            await _context.Entry(trackedEntity).ReloadAsync(cancellationToken);
            _productLogger.LogDebug("Reloaded product: {ProductName} with updated stock: {StockQuantity}", 
                trackedEntity.Name, trackedEntity.StockQuantity);
            
            return trackedEntity;
        }
        
        // If not tracked, get fresh data and attach
        var product = await _dbSet
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
        if (product != null)
        {
            _productLogger.LogDebug("Found product: {ProductName} with stock: {StockQuantity}", 
                product.Name, product.StockQuantity);
        }
        else
        {
            _productLogger.LogWarning("Product not found: {ProductId}", id);
        }
        
        return product;
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
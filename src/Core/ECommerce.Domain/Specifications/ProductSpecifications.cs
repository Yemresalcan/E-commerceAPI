using System.Linq.Expressions;
using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Domain.Specifications;

/// <summary>
/// Specification for active products
/// </summary>
public class ActiveProductsSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> Criteria => 
        product => product.IsActive;
}

/// <summary>
/// Specification for featured products
/// </summary>
public class FeaturedProductsSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> Criteria => 
        product => product.IsFeatured && product.IsActive;
}

/// <summary>
/// Specification for products in a specific category
/// </summary>
public class ProductsByCategorySpecification : Specification<Product>
{
    private readonly Guid _categoryId;

    public ProductsByCategorySpecification(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => product.CategoryId == _categoryId && product.IsActive;
}

/// <summary>
/// Specification for products with low stock
/// </summary>
public class LowStockProductsSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> Criteria => 
        product => product.StockQuantity <= product.MinimumStockLevel && product.StockQuantity > 0;
}

/// <summary>
/// Specification for out-of-stock products
/// </summary>
public class OutOfStockProductsSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> Criteria => 
        product => product.StockQuantity <= 0;
}

/// <summary>
/// Specification for products by SKU
/// </summary>
public class ProductBySkuSpecification : Specification<Product>
{
    private readonly string _sku;

    public ProductBySkuSpecification(string sku)
    {
        _sku = sku ?? throw new ArgumentNullException(nameof(sku));
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => product.Sku == _sku;
}

/// <summary>
/// Specification for searching products by name or description
/// </summary>
public class ProductSearchSpecification : Specification<Product>
{
    private readonly string _searchTerm;

    public ProductSearchSpecification(string searchTerm)
    {
        _searchTerm = searchTerm?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(searchTerm));
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => product.IsActive && 
                   (product.Name.ToLower().Contains(_searchTerm) || 
                    product.Description.ToLower().Contains(_searchTerm));
}

/// <summary>
/// Specification for products within a price range
/// </summary>
public class ProductsByPriceRangeSpecification : Specification<Product>
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;
    private readonly string _currency;

    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice, string currency)
    {
        _minPrice = minPrice;
        _maxPrice = maxPrice;
        _currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => product.IsActive && 
                   product.Price.Currency == _currency &&
                   product.Price.Amount >= _minPrice && 
                   product.Price.Amount <= _maxPrice;
}

/// <summary>
/// Specification for products with minimum stock quantity
/// </summary>
public class ProductsWithMinimumStockSpecification : Specification<Product>
{
    private readonly int _minimumStock;

    public ProductsWithMinimumStockSpecification(int minimumStock)
    {
        _minimumStock = minimumStock;
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => product.StockQuantity >= _minimumStock;
}

/// <summary>
/// Specification for products by multiple IDs
/// </summary>
public class ProductsByIdsSpecification : Specification<Product>
{
    private readonly IEnumerable<Guid> _productIds;

    public ProductsByIdsSpecification(IEnumerable<Guid> productIds)
    {
        _productIds = productIds ?? throw new ArgumentNullException(nameof(productIds));
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => _productIds.Contains(product.Id);
}

/// <summary>
/// Specification for products with reviews above a certain rating
/// </summary>
public class ProductsWithHighRatingSpecification : Specification<Product>
{
    private readonly decimal _minimumRating;

    public ProductsWithHighRatingSpecification(decimal minimumRating)
    {
        _minimumRating = minimumRating;
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => product.IsActive && product.AverageRating >= _minimumRating;
}

/// <summary>
/// Specification for paginated products with sorting
/// </summary>
public class PaginatedProductsSpecification : Specification<Product>
{
    public PaginatedProductsSpecification(int pageNumber, int pageSize, bool orderByName = true)
    {
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        
        if (orderByName)
        {
            ApplyOrderBy(product => product.Name);
        }
        else
        {
            ApplyOrderByDescending(product => product.CreatedAt);
        }
    }

    public override Expression<Func<Product, bool>> Criteria => 
        product => product.IsActive;
}
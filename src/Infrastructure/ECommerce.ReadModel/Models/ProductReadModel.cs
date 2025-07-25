using Nest;

namespace ECommerce.ReadModel.Models;

/// <summary>
/// Read model for Product optimized for search and query operations
/// </summary>
[ElasticsearchType(IdProperty = nameof(Id))]
public class ProductReadModel
{
    /// <summary>
    /// Product unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Product name - searchable text
    /// </summary>
    [Text(Analyzer = "standard")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description - searchable text
    /// </summary>
    [Text(Analyzer = "standard")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU - exact match
    /// </summary>
    [Keyword]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product price amount
    /// </summary>
    [Number(NumberType.Double)]
    public decimal Price { get; set; }

    /// <summary>
    /// Price currency code
    /// </summary>
    [Keyword]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Current stock quantity
    /// </summary>
    [Number(NumberType.Integer)]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Minimum stock level
    /// </summary>
    [Number(NumberType.Integer)]
    public int MinimumStockLevel { get; set; }

    /// <summary>
    /// Product category information
    /// </summary>
    [Object]
    public CategoryReadModel Category { get; set; } = new();

    /// <summary>
    /// Whether the product is active
    /// </summary>
    [Boolean]
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether the product is featured
    /// </summary>
    [Boolean]
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Product weight in grams
    /// </summary>
    [Number(NumberType.Double)]
    public decimal Weight { get; set; }

    /// <summary>
    /// Product dimensions
    /// </summary>
    [Text]
    public string Dimensions { get; set; } = string.Empty;

    /// <summary>
    /// Average rating from reviews
    /// </summary>
    [Number(NumberType.Double)]
    public decimal AverageRating { get; set; }

    /// <summary>
    /// Total number of reviews
    /// </summary>
    [Number(NumberType.Integer)]
    public int ReviewCount { get; set; }

    /// <summary>
    /// Whether product is in stock
    /// </summary>
    [Boolean]
    public bool IsInStock { get; set; }

    /// <summary>
    /// Whether product stock is low
    /// </summary>
    [Boolean]
    public bool IsLowStock { get; set; }

    /// <summary>
    /// Whether product is out of stock
    /// </summary>
    [Boolean]
    public bool IsOutOfStock { get; set; }

    /// <summary>
    /// Product creation date
    /// </summary>
    [Date]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Product last update date
    /// </summary>
    [Date]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Tags for enhanced searchability
    /// </summary>
    [Keyword]
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Search suggestions for autocomplete
    /// </summary>
    [Completion]
    public CompletionField Suggest { get; set; } = new();
}

/// <summary>
/// Category information for product read model
/// </summary>
public class CategoryReadModel
{
    /// <summary>
    /// Category unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    [Text(Analyzer = "standard")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    [Text]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Parent category ID for hierarchical structure
    /// </summary>
    [Keyword]
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Category path for hierarchical navigation
    /// </summary>
    [Keyword]
    public string CategoryPath { get; set; } = string.Empty;
}
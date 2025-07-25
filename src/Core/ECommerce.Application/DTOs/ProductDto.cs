namespace ECommerce.Application.DTOs;

/// <summary>
/// Data transfer object for product information
/// </summary>
public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    decimal Price,
    string Currency,
    int StockQuantity,
    CategoryDto Category,
    bool IsActive,
    bool IsFeatured,
    decimal Weight,
    string Dimensions,
    decimal AverageRating,
    int ReviewCount,
    bool IsInStock,
    bool IsLowStock,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Tags
);

/// <summary>
/// Data transfer object for category information
/// </summary>
public record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    Guid? ParentCategoryId,
    string CategoryPath
);
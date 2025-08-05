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
    int MinimumStockLevel,
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
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public bool IsRoot { get; set; }
    public bool HasChildren { get; set; }
    public IEnumerable<CategoryDto>? Children { get; set; }
}
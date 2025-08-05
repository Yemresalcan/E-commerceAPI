using ECommerce.Domain.Aggregates.ProductAggregate;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for Category aggregate
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Get categories by parent ID
    /// </summary>
    Task<IEnumerable<Category>> GetByParentIdAsync(Guid? parentId, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all root categories
    /// </summary>
    Task<IEnumerable<Category>> GetRootCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get category hierarchy starting from a specific category
    /// </summary>
    Task<Category?> GetWithChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if category has products
    /// </summary>
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
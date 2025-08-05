using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Category aggregate
/// </summary>
public class CategoryRepository(
    ECommerceDbContext context,
    ILogger<CategoryRepository> logger,
    ILogger<Repository<Category>> baseLogger) : Repository<Category>(context, baseLogger), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetByParentIdAsync(Guid? parentId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting categories by parent ID: {ParentId}, IncludeInactive: {IncludeInactive}", parentId, includeInactive);

        var query = _dbSet.Where(c => c.ParentCategoryId == parentId);

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting root categories, IncludeInactive: {IncludeInactive}", includeInactive);

        var query = _dbSet.Where(c => c.ParentCategoryId == null);

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetWithChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting category with children: {CategoryId}", categoryId);

        return await _dbSet
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Checking if category has products: {CategoryId}", categoryId);

        return await _context.Set<Product>()
            .AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
    }
}
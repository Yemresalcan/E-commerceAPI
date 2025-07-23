using ECommerce.Domain.Aggregates;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation using Entity Framework Core
/// </summary>
/// <typeparam name="T">The aggregate root type</typeparam>
public class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly ECommerceDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Gets an aggregate by its unique identifier
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    /// <summary>
    /// Gets all aggregates
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new aggregate to the repository
    /// </summary>
    public virtual async Task AddAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        await _dbSet.AddAsync(aggregate, cancellationToken);
    }

    /// <summary>
    /// Updates an existing aggregate in the repository
    /// </summary>
    public virtual void Update(T aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        _dbSet.Update(aggregate);
    }

    /// <summary>
    /// Removes an aggregate from the repository
    /// </summary>
    public virtual void Delete(T aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        _dbSet.Remove(aggregate);
    }

    /// <summary>
    /// Removes an aggregate by its identifier
    /// </summary>
    public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    /// <summary>
    /// Checks if an aggregate exists with the given identifier
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets aggregates that satisfy the given specification
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a single aggregate that satisfies the given specification
    /// </summary>
    public virtual async Task<T?> FindSingleAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Counts the number of aggregates that satisfy the given specification
    /// </summary>
    public virtual async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    /// <summary>
    /// Applies the specification to the queryable
    /// </summary>
    protected virtual IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var query = _dbSet.AsQueryable();

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply grouping
        if (specification.GroupBy != null)
        {
            // Note: GroupBy in specifications is complex and might need specific implementation per use case
            // For now, we'll skip this as it's not commonly used in basic CRUD operations
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
            {
                query = query.Skip(specification.Skip.Value);
            }

            if (specification.Take.HasValue)
            {
                query = query.Take(specification.Take.Value);
            }
        }

        return query;
    }
}
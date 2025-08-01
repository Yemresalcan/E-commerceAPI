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
    protected readonly ILogger<Repository<T>> _logger;

    public Repository(ECommerceDbContext context, ILogger<Repository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets an aggregate by its unique identifier with performance optimizations
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using (_logger.BeginRepositoryScope(typeof(T).Name, "GetById"))
        {
            _logger.LogDebug("Getting {AggregateType} by ID: {AggregateId}", typeof(T).Name, id);
            
            // Use FindAsync for primary key lookups - it's optimized and checks local cache first
            var result = await _dbSet.FindAsync([id], cancellationToken);
            
            if (result == null)
            {
                _logger.LogDebug("{AggregateType} with ID {AggregateId} not found", typeof(T).Name, id);
            }
            else
            {
                _logger.LogDebug("Successfully retrieved {AggregateType} with ID: {AggregateId}", typeof(T).Name, id);
            }
            
            return result;
        }
    }

    /// <summary>
    /// Gets all aggregates with performance optimizations
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using (_logger.BeginRepositoryScope(typeof(T).Name, "GetAll"))
        {
            _logger.LogDebug("Getting all {AggregateType} entities", typeof(T).Name);
            
            // Use AsNoTracking for read-only operations to improve performance
            var result = await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
            
            _logger.LogDebug("Retrieved {Count} {AggregateType} entities", result.Count, typeof(T).Name);
            return result;
        }
    }

    /// <summary>
    /// Adds a new aggregate to the repository
    /// </summary>
    public virtual async Task AddAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        
        using (_logger.BeginRepositoryScope(typeof(T).Name, "Add"))
        {
            _logger.LogDebug("Adding new {AggregateType} with ID: {AggregateId}", typeof(T).Name, aggregate.Id);
            await _dbSet.AddAsync(aggregate, cancellationToken);
            _logger.LogDebug("Successfully added {AggregateType} with ID: {AggregateId} to context", typeof(T).Name, aggregate.Id);
        }
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
    /// Checks if an aggregate exists with the given identifier (optimized for performance)
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using (_logger.BeginRepositoryScope(typeof(T).Name, "Exists"))
        {
            // Use AsNoTracking for existence checks to improve performance
            var exists = await _dbSet.AsNoTracking().AnyAsync(e => e.Id == id, cancellationToken);
            _logger.LogDebug("{AggregateType} with ID {AggregateId} exists: {Exists}", typeof(T).Name, id, exists);
            return exists;
        }
    }

    /// <summary>
    /// Gets multiple aggregates by their identifiers (batch operation for performance)
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        
        var idList = ids.ToList();
        if (!idList.Any())
            return [];

        using (_logger.BeginRepositoryScope(typeof(T).Name, "GetByIds"))
        {
            _logger.LogDebug("Getting {Count} {AggregateType} entities by IDs", idList.Count, typeof(T).Name);
            
            var result = await _dbSet
                .Where(e => idList.Contains(e.Id))
                .ToListAsync(cancellationToken);
            
            _logger.LogDebug("Retrieved {Count} {AggregateType} entities", result.Count, typeof(T).Name);
            return result;
        }
    }

    /// <summary>
    /// Gets a page of aggregates with performance optimizations
    /// </summary>
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        using (_logger.BeginRepositoryScope(typeof(T).Name, "GetPaged"))
        {
            _logger.LogDebug("Getting page {PageNumber} of {AggregateType} entities (page size: {PageSize})", 
                pageNumber, typeof(T).Name, pageSize);

            // Get total count efficiently
            var totalCount = await _dbSet.CountAsync(cancellationToken);
            
            // Get paged results with no tracking for better performance
            var items = await _dbSet
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} {AggregateType} entities (total: {TotalCount})", 
                items.Count, typeof(T).Name, totalCount);

            return (items, totalCount);
        }
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
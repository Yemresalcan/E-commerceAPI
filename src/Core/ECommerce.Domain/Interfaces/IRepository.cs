using ECommerce.Domain.Aggregates;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Base repository interface for aggregate roots
/// </summary>
/// <typeparam name="T">The aggregate root type</typeparam>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>
    /// Gets an aggregate by its unique identifier
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The aggregate if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all aggregates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all aggregates</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository
    /// </summary>
    /// <param name="aggregate">The aggregate to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(T aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate in the repository
    /// </summary>
    /// <param name="aggregate">The aggregate to update</param>
    void Update(T aggregate);

    /// <summary>
    /// Removes an aggregate from the repository
    /// </summary>
    /// <param name="aggregate">The aggregate to remove</param>
    void Delete(T aggregate);

    /// <summary>
    /// Removes an aggregate by its identifier
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an aggregate exists with the given identifier
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the aggregate exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aggregates that satisfy the given specification
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of aggregates that satisfy the specification</returns>
    Task<IEnumerable<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single aggregate that satisfies the given specification
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The first aggregate that satisfies the specification, or null if none found</returns>
    Task<T?> FindSingleAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of aggregates that satisfy the given specification
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The count of aggregates that satisfy the specification</returns>
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
}
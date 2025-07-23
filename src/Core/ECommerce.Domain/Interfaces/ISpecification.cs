using System.Linq.Expressions;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Specification pattern interface for encapsulating query logic
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// The criteria expression that defines the specification
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Include expressions for eager loading related entities as strings
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression for ascending sort
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by expression for descending sort
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Group by expression
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }

    /// <summary>
    /// Number of items to take (for pagination)
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Number of items to skip (for pagination)
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Whether the specification is satisfied by the given entity
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the entity satisfies the specification, false otherwise</returns>
    bool IsSatisfiedBy(T entity);
}
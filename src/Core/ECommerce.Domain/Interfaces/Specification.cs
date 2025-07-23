using System.Linq.Expressions;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Base implementation of the specification pattern
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>
    /// The criteria expression that defines the specification
    /// </summary>
    public abstract Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading related entities
    /// </summary>
    public List<Expression<Func<T, object>>> Includes { get; } = [];

    /// <summary>
    /// Include expressions for eager loading related entities as strings
    /// </summary>
    public List<string> IncludeStrings { get; } = [];

    /// <summary>
    /// Order by expression for ascending sort
    /// </summary>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <summary>
    /// Order by expression for descending sort
    /// </summary>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <summary>
    /// Group by expression
    /// </summary>
    public Expression<Func<T, object>>? GroupBy { get; private set; }

    /// <summary>
    /// Number of items to take (for pagination)
    /// </summary>
    public int? Take { get; private set; }

    /// <summary>
    /// Number of items to skip (for pagination)
    /// </summary>
    public int? Skip { get; private set; }

    /// <summary>
    /// Whether paging is enabled
    /// </summary>
    public bool IsPagingEnabled => Skip.HasValue;

    /// <summary>
    /// Whether the specification is satisfied by the given entity
    /// </summary>
    /// <param name="entity">The entity to check</param>
    /// <returns>True if the entity satisfies the specification, false otherwise</returns>
    public virtual bool IsSatisfiedBy(T entity)
    {
        return Criteria.Compile()(entity);
    }

    /// <summary>
    /// Adds an include expression for eager loading
    /// </summary>
    /// <param name="includeExpression">The include expression</param>
    /// <returns>The specification for method chaining</returns>
    protected virtual ISpecification<T> AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
        return this;
    }

    /// <summary>
    /// Adds an include string for eager loading
    /// </summary>
    /// <param name="includeString">The include string</param>
    /// <returns>The specification for method chaining</returns>
    protected virtual ISpecification<T> AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
        return this;
    }

    /// <summary>
    /// Applies ascending ordering
    /// </summary>
    /// <param name="orderByExpression">The order by expression</param>
    /// <returns>The specification for method chaining</returns>
    protected virtual ISpecification<T> ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        return this;
    }

    /// <summary>
    /// Applies descending ordering
    /// </summary>
    /// <param name="orderByDescExpression">The order by descending expression</param>
    /// <returns>The specification for method chaining</returns>
    protected virtual ISpecification<T> ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
        return this;
    }

    /// <summary>
    /// Applies grouping
    /// </summary>
    /// <param name="groupByExpression">The group by expression</param>
    /// <returns>The specification for method chaining</returns>
    protected virtual ISpecification<T> ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
        return this;
    }

    /// <summary>
    /// Applies paging
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>The specification for method chaining</returns>
    protected virtual ISpecification<T> ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        return this;
    }
}
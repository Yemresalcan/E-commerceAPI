namespace ECommerce.Application.Common.Models;

/// <summary>
/// Represents a paged result set
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
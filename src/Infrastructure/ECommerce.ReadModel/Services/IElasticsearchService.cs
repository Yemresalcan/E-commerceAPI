using Nest;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Base interface for Elasticsearch operations
/// </summary>
public interface IElasticsearchService<T> where T : class
{
    /// <summary>
    /// Creates or updates a document in the index
    /// </summary>
    Task<bool> IndexDocumentAsync(T document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates multiple documents in the index
    /// </summary>
    Task<bool> IndexDocumentsAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a document by its ID
    /// </summary>
    Task<T?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document by its ID
    /// </summary>
    Task<bool> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches documents with the specified query
    /// </summary>
    Task<ISearchResponse<T>> SearchAsync(SearchDescriptor<T> searchDescriptor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a simple text search across specified fields
    /// </summary>
    Task<ISearchResponse<T>> SimpleSearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suggestions for autocomplete functionality
    /// </summary>
    Task<ISearchResponse<T>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the index exists
    /// </summary>
    Task<bool> IndexExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the index with proper mappings
    /// </summary>
    Task<bool> CreateIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the index
    /// </summary>
    Task<bool> DeleteIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the index to make recent changes searchable
    /// </summary>
    Task<bool> RefreshIndexAsync(CancellationToken cancellationToken = default);
}
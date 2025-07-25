namespace ECommerce.ReadModel.Services;

/// <summary>
/// Interface for managing Elasticsearch indices
/// </summary>
public interface IIndexManagementService
{
    /// <summary>
    /// Ensures all required indices are created
    /// </summary>
    Task<bool> EnsureAllIndicesCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates all indices if they don't exist
    /// </summary>
    Task<bool> CreateAllIndicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all indices
    /// </summary>
    Task<bool> DeleteAllIndicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Recreates all indices (deletes and creates)
    /// </summary>
    Task<bool> RecreateAllIndicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the health of all indices
    /// </summary>
    Task<IndexHealthStatus> CheckIndicesHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes all indices
    /// </summary>
    Task<bool> RefreshAllIndicesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Health status of indices
/// </summary>
public class IndexHealthStatus
{
    public bool IsHealthy { get; set; }
    public Dictionary<string, bool> IndexStatus { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
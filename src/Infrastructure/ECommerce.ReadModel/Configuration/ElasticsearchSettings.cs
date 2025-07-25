namespace ECommerce.ReadModel.Configuration;

/// <summary>
/// Configuration settings for Elasticsearch connection
/// </summary>
public class ElasticsearchSettings
{
    public const string SectionName = "Elasticsearch";

    /// <summary>
    /// Elasticsearch connection URI
    /// </summary>
    public string Uri { get; set; } = "http://localhost:9200";

    /// <summary>
    /// Default index name prefix
    /// </summary>
    public string IndexPrefix { get; set; } = "ecommerce";

    /// <summary>
    /// Username for authentication (optional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for authentication (optional)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Enable debug mode for detailed logging
    /// </summary>
    public bool EnableDebugMode { get; set; } = false;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retries for failed requests
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
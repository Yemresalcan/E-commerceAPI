using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;

namespace ECommerce.ReadModel.Configuration;

/// <summary>
/// Factory for creating and configuring Elasticsearch client
/// </summary>
public class ElasticsearchClientFactory
{
    private readonly ElasticsearchSettings _settings;
    private readonly ILogger<ElasticsearchClientFactory> _logger;

    public ElasticsearchClientFactory(
        IOptions<ElasticsearchSettings> settings,
        ILogger<ElasticsearchClientFactory> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Creates a configured Elasticsearch client
    /// </summary>
    public IElasticClient CreateClient()
    {
        var uri = new Uri(_settings.Uri);
        var connectionSettings = new ConnectionSettings(uri)
            .DefaultIndex($"{_settings.IndexPrefix}-products")
            .RequestTimeout(TimeSpan.FromSeconds(_settings.TimeoutSeconds))
            .MaximumRetries(_settings.MaxRetries)
            .ThrowExceptions(false)
            .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
            .EnableHttpCompression(false);

        // Configure authentication if provided
        if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
        {
            connectionSettings.BasicAuthentication(_settings.Username, _settings.Password);
        }

        // Configure debug mode
        if (_settings.EnableDebugMode)
        {
            connectionSettings.EnableDebugMode()
                .PrettyJson()
                .DisableDirectStreaming();
        }

        // Configure logging
        connectionSettings.OnRequestCompleted(response =>
        {
            if (response.Success)
            {
                _logger.LogDebug("Elasticsearch request completed successfully: {Method} {Uri}",
                    response.HttpMethod, response.Uri);
            }
            else
            {
                _logger.LogWarning("Elasticsearch request failed: {Method} {Uri} - {Error}",
                    response.HttpMethod, response.Uri, response.OriginalException?.Message ?? "Unknown error");
            }
        });

        return new ElasticClient(connectionSettings);
    }
}
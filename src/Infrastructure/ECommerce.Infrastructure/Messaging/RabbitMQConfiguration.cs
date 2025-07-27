namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// Configuration settings for RabbitMQ connection and messaging
/// </summary>
public class RabbitMQConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ server hostname
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ server port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Username for RabbitMQ authentication
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password for RabbitMQ authentication
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual host to use
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Exchange name for domain events
    /// </summary>
    public string ExchangeName { get; set; } = "ecommerce.domain.events";

    /// <summary>
    /// Exchange type (topic, direct, fanout, headers)
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Queue name prefix for event handlers
    /// </summary>
    public string QueueNamePrefix { get; set; } = "ecommerce.events";

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeout { get; set; } = 30;

    /// <summary>
    /// Whether to enable automatic recovery
    /// </summary>
    public bool AutomaticRecoveryEnabled { get; set; } = true;

    /// <summary>
    /// Network recovery interval in seconds
    /// </summary>
    public int NetworkRecoveryInterval { get; set; } = 10;
}
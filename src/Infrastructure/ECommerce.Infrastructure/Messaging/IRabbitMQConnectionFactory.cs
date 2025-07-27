using RabbitMQ.Client;

namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// Interface for creating RabbitMQ connections
/// </summary>
public interface IRabbitMQConnectionFactory
{
    /// <summary>
    /// Creates a new RabbitMQ connection
    /// </summary>
    /// <returns>RabbitMQ connection</returns>
    IConnection CreateConnection();

    /// <summary>
    /// Creates a new RabbitMQ channel from an existing connection
    /// </summary>
    /// <param name="connection">The connection to create channel from</param>
    /// <returns>RabbitMQ channel</returns>
    IModel CreateChannel(IConnection connection);
}
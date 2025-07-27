namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for publishing and subscribing to domain events through a message bus
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes a domain event to the message bus
    /// </summary>
    /// <typeparam name="T">The type of domain event</typeparam>
    /// <param name="domainEvent">The domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : class;

    /// <summary>
    /// Subscribes to a specific type of domain event
    /// </summary>
    /// <typeparam name="T">The type of domain event to subscribe to</typeparam>
    /// <param name="handler">The handler function to process the event</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler) 
        where T : class;

    /// <summary>
    /// Starts the event bus consumer to begin processing messages
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the event bus consumer
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
}
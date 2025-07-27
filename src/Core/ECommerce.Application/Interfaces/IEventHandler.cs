namespace ECommerce.Application.Interfaces;

/// <summary>
/// Base interface for all domain event handlers
/// </summary>
/// <typeparam name="T">The type of domain event to handle</typeparam>
public interface IEventHandler<in T> where T : class
{
    /// <summary>
    /// Handles the specified domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
}
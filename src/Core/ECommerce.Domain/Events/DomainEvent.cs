namespace ECommerce.Domain.Events;

/// <summary>
/// Base abstract record for all domain events in the system.
/// Domain events represent something important that happened in the domain.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Unique identifier for the domain event
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The date and time when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The version of the event for potential future schema evolution
    /// </summary>
    public int Version { get; init; } = 1;
}
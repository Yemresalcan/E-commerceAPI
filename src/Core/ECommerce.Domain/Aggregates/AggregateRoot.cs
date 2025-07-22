using ECommerce.Domain.Events;

namespace ECommerce.Domain.Aggregates;

/// <summary>
/// Base class for all aggregate roots in the domain.
/// Provides domain event collection and management functionality.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// Unique identifier for the aggregate
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Version of the aggregate for optimistic concurrency control
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    /// Date and time when the aggregate was created
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Date and time when the aggregate was last updated
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    /// <summary>
    /// Read-only collection of domain events that have been raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the collection of events for this aggregate
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event from the collection
    /// </summary>
    /// <param name="domainEvent">The domain event to remove</param>
    protected void RemoveDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the collection.
    /// This is typically called after events have been published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks the aggregate as modified by updating the UpdatedAt timestamp and incrementing the version
    /// </summary>
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Initializes the aggregate with basic properties
    /// </summary>
    protected AggregateRoot()
    {
        Id = Guid.NewGuid();
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes the aggregate with a specific ID (used for reconstitution from storage)
    /// </summary>
    /// <param name="id">The aggregate identifier</param>
    protected AggregateRoot(Guid id)
    {
        Id = id;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
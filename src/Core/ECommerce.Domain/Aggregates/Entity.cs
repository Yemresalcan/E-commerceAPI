namespace ECommerce.Domain.Aggregates;

/// <summary>
/// Base class for all entities in the domain.
/// Provides identity and equality comparison functionality.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Date and time when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    /// <summary>
    /// Initializes the entity with a new unique identifier
    /// </summary>
    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes the entity with a specific identifier (used for reconstitution from storage)
    /// </summary>
    /// <param name="id">The entity identifier</param>
    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as modified by updating the UpdatedAt timestamp
    /// </summary>
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the specified entity is equal to the current entity
    /// </summary>
    /// <param name="other">The entity to compare with the current entity</param>
    /// <returns>true if the specified entity is equal to the current entity; otherwise, false</returns>
    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity
    /// </summary>
    /// <param name="obj">The object to compare with the current entity</param>
    /// <returns>true if the specified object is equal to the current entity; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    /// <summary>
    /// Returns the hash code for this entity
    /// </summary>
    /// <returns>A hash code for the current entity</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Determines whether two entities are equal
    /// </summary>
    /// <param name="left">The first entity to compare</param>
    /// <param name="right">The second entity to compare</param>
    /// <returns>true if the entities are equal; otherwise, false</returns>
    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two entities are not equal
    /// </summary>
    /// <param name="left">The first entity to compare</param>
    /// <param name="right">The second entity to compare</param>
    /// <returns>true if the entities are not equal; otherwise, false</returns>
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }
}
namespace ECommerce.Domain.Events;

/// <summary>
/// Domain event raised when a new customer registers in the system
/// </summary>
public record CustomerRegisteredEvent : DomainEvent
{
    /// <summary>
    /// The unique identifier of the registered customer
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// The email address of the registered customer
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The first name of the registered customer
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// The last name of the registered customer
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// The phone number of the registered customer (optional)
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// The registration date and time
    /// </summary>
    public DateTime RegistrationDate { get; init; }

    public CustomerRegisteredEvent(
        Guid customerId,
        string email,
        string firstName,
        string lastName,
        string? phoneNumber = null)
    {
        CustomerId = customerId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        RegistrationDate = DateTime.UtcNow;
    }
}
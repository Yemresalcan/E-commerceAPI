using ECommerce.Domain.Events;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Aggregates.CustomerAggregate;

/// <summary>
/// Customer aggregate root representing a customer in the e-commerce system.
/// Manages customer identity, profile, and addresses.
/// </summary>
public class Customer : AggregateRoot
{
    private readonly List<Address> _addresses = [];

    /// <summary>
    /// Customer's first name
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Customer's last name
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Customer's email address (unique identifier)
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Customer's phone number (optional)
    /// </summary>
    public PhoneNumber? PhoneNumber { get; private set; }

    /// <summary>
    /// Customer's profile containing preferences and settings
    /// </summary>
    public Profile Profile { get; private set; } = null!;

    /// <summary>
    /// Indicates whether the customer account is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Date when the customer registered
    /// </summary>
    public DateTime RegistrationDate { get; private set; }

    /// <summary>
    /// Date when the customer was last active (optional)
    /// </summary>
    public DateTime? LastActiveDate { get; private set; }

    /// <summary>
    /// Read-only collection of customer addresses
    /// </summary>
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    /// <summary>
    /// Gets the customer's primary address (first address marked as primary, or first address if none marked)
    /// </summary>
    public Address? PrimaryAddress => _addresses.FirstOrDefault(a => a.IsPrimary) ?? _addresses.FirstOrDefault();

    /// <summary>
    /// Gets the customer's full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Private constructor for EF Core
    private Customer() { }

    /// <summary>
    /// Creates a new customer with the specified details
    /// </summary>
    /// <param name="firstName">Customer's first name</param>
    /// <param name="lastName">Customer's last name</param>
    /// <param name="email">Customer's email address</param>
    /// <param name="phoneNumber">Customer's phone number (optional)</param>
    /// <returns>A new Customer instance</returns>
    public static Customer Create(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phoneNumber = null)
    {
        ValidateCustomerCreation(firstName, lastName, email);

        var customer = new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            PhoneNumber = phoneNumber,
            Profile = Profile.CreateDefault(),
            IsActive = true,
            RegistrationDate = DateTime.UtcNow,
            LastActiveDate = DateTime.UtcNow
        };

        customer.AddDomainEvent(new CustomerRegisteredEvent(
            customer.Id,
            customer.Email.Value,
            customer.FirstName,
            customer.LastName,
            customer.PhoneNumber?.Value));

        return customer;
    }

    /// <summary>
    /// Updates the customer's basic information
    /// </summary>
    /// <param name="firstName">New first name</param>
    /// <param name="lastName">New last name</param>
    /// <param name="phoneNumber">New phone number (optional)</param>
    public void UpdateBasicInfo(string firstName, string lastName, PhoneNumber? phoneNumber = null)
    {
        ValidateBasicInfo(firstName, lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = phoneNumber;

        MarkAsModified();
    }

    /// <summary>
    /// Updates the customer's email address
    /// </summary>
    /// <param name="newEmail">New email address</param>
    public void UpdateEmail(Email newEmail)
    {
        ArgumentNullException.ThrowIfNull(newEmail);

        if (Email.Value.Equals(newEmail.Value, StringComparison.OrdinalIgnoreCase))
            return;

        Email = newEmail;
        MarkAsModified();
    }

    /// <summary>
    /// Adds a new address to the customer
    /// </summary>
    /// <param name="address">Address to add</param>
    public void AddAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        // Ensure only one primary address exists
        if (address.IsPrimary)
        {
            foreach (var existingAddress in _addresses)
            {
                existingAddress.SetAsPrimary(false);
            }
        }

        // If this is the first address, make it primary
        if (_addresses.Count == 0)
        {
            address.SetAsPrimary(true);
        }

        _addresses.Add(address);
        MarkAsModified();
    }

    /// <summary>
    /// Removes an address from the customer
    /// </summary>
    /// <param name="addressId">ID of the address to remove</param>
    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
            throw new InvalidOperationException($"Address with ID {addressId} not found.");

        var wasPrimary = address.IsPrimary;
        _addresses.Remove(address);

        // If we removed the primary address and there are other addresses, make the first one primary
        if (wasPrimary && _addresses.Count > 0)
        {
            _addresses.First().SetAsPrimary(true);
        }

        MarkAsModified();
    }

    /// <summary>
    /// Sets an address as the primary address
    /// </summary>
    /// <param name="addressId">ID of the address to set as primary</param>
    public void SetPrimaryAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
            throw new InvalidOperationException($"Address with ID {addressId} not found.");

        // Remove primary flag from all addresses
        foreach (var existingAddress in _addresses)
        {
            existingAddress.SetAsPrimary(false);
        }

        // Set the specified address as primary
        address.SetAsPrimary(true);
        MarkAsModified();
    }

    /// <summary>
    /// Updates the customer's profile
    /// </summary>
    /// <param name="profile">New profile information</param>
    public void UpdateProfile(Profile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        Profile = profile;
        MarkAsModified();
    }

    /// <summary>
    /// Deactivates the customer account
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        MarkAsModified();
    }

    /// <summary>
    /// Reactivates the customer account
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastActiveDate = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// Records customer activity
    /// </summary>
    public void RecordActivity()
    {
        LastActiveDate = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// Gets addresses by type
    /// </summary>
    /// <param name="type">Address type to filter by</param>
    /// <returns>Collection of addresses of the specified type</returns>
    public IEnumerable<Address> GetAddressesByType(AddressType type)
    {
        return _addresses.Where(a => a.Type == type);
    }

    /// <summary>
    /// Checks if the customer has any addresses
    /// </summary>
    public bool HasAddresses => _addresses.Count > 0;

    /// <summary>
    /// Checks if the customer has a primary address
    /// </summary>
    public bool HasPrimaryAddress => _addresses.Any(a => a.IsPrimary);

    private static void ValidateCustomerCreation(string firstName, string lastName, Email email)
    {
        ValidateBasicInfo(firstName, lastName);
        ArgumentNullException.ThrowIfNull(email);
    }

    private static void ValidateBasicInfo(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));

        if (firstName.Trim().Length > 50)
            throw new ArgumentException("First name cannot exceed 50 characters.", nameof(firstName));

        if (lastName.Trim().Length > 50)
            throw new ArgumentException("Last name cannot exceed 50 characters.", nameof(lastName));
    }
}
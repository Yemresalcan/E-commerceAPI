namespace ECommerce.Domain.Aggregates.CustomerAggregate;

/// <summary>
/// Represents the type of address
/// </summary>
public enum AddressType
{
    Shipping,
    Billing,
    Both
}

/// <summary>
/// Address entity representing a customer's address with validation
/// </summary>
public class Address : Entity
{
    /// <summary>
    /// The customer this address belongs to
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Address type (shipping, billing, or both)
    /// </summary>
    public AddressType Type { get; private set; }

    /// <summary>
    /// Street address line 1
    /// </summary>
    public string Street1 { get; private set; } = string.Empty;

    /// <summary>
    /// Street address line 2 (optional)
    /// </summary>
    public string? Street2 { get; private set; }

    /// <summary>
    /// City name
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// State or province
    /// </summary>
    public string State { get; private set; } = string.Empty;

    /// <summary>
    /// Postal or ZIP code
    /// </summary>
    public string PostalCode { get; private set; } = string.Empty;

    /// <summary>
    /// Country name or code
    /// </summary>
    public string Country { get; private set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is the primary address
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Optional label for the address (e.g., "Home", "Office")
    /// </summary>
    public string? Label { get; private set; }

    /// <summary>
    /// Gets the full address as a formatted string
    /// </summary>
    public string FullAddress
    {
        get
        {
            var parts = new List<string> { Street1 };
            
            if (!string.IsNullOrWhiteSpace(Street2))
                parts.Add(Street2);
            
            parts.Add($"{City}, {State} {PostalCode}");
            parts.Add(Country);
            
            return string.Join(Environment.NewLine, parts);
        }
    }

    /// <summary>
    /// Gets a single-line representation of the address
    /// </summary>
    public string SingleLineAddress
    {
        get
        {
            var parts = new List<string> { Street1 };
            
            if (!string.IsNullOrWhiteSpace(Street2))
                parts.Add(Street2);
            
            parts.Add($"{City}, {State} {PostalCode}, {Country}");
            
            return string.Join(", ", parts);
        }
    }

    // Private constructor for EF Core
    private Address() { }

    /// <summary>
    /// Creates a new address with the specified details
    /// </summary>
    /// <param name="type">Address type</param>
    /// <param name="street1">Street address line 1</param>
    /// <param name="city">City name</param>
    /// <param name="state">State or province</param>
    /// <param name="postalCode">Postal or ZIP code</param>
    /// <param name="country">Country name or code</param>
    /// <param name="street2">Street address line 2 (optional)</param>
    /// <param name="label">Address label (optional)</param>
    /// <param name="isPrimary">Whether this is the primary address</param>
    /// <returns>A new Address instance</returns>
    public static Address Create(
        AddressType type,
        string street1,
        string city,
        string state,
        string postalCode,
        string country,
        string? street2 = null,
        string? label = null,
        bool isPrimary = false)
    {
        ValidateAddressCreation(street1, city, state, postalCode, country);

        return new Address
        {
            Type = type,
            Street1 = street1.Trim(),
            Street2 = string.IsNullOrWhiteSpace(street2) ? null : street2.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            PostalCode = postalCode.Trim(),
            Country = country.Trim(),
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
            IsPrimary = isPrimary
        };
    }

    /// <summary>
    /// Updates the address details
    /// </summary>
    /// <param name="type">Address type</param>
    /// <param name="street1">Street address line 1</param>
    /// <param name="city">City name</param>
    /// <param name="state">State or province</param>
    /// <param name="postalCode">Postal or ZIP code</param>
    /// <param name="country">Country name or code</param>
    /// <param name="street2">Street address line 2 (optional)</param>
    /// <param name="label">Address label (optional)</param>
    public void Update(
        AddressType type,
        string street1,
        string city,
        string state,
        string postalCode,
        string country,
        string? street2 = null,
        string? label = null)
    {
        ValidateAddressCreation(street1, city, state, postalCode, country);

        Type = type;
        Street1 = street1.Trim();
        Street2 = string.IsNullOrWhiteSpace(street2) ? null : street2.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();

        MarkAsModified();
    }

    /// <summary>
    /// Sets or removes the primary flag for this address
    /// </summary>
    /// <param name="isPrimary">Whether this address should be primary</param>
    internal void SetAsPrimary(bool isPrimary)
    {
        if (IsPrimary == isPrimary)
            return;

        IsPrimary = isPrimary;
        MarkAsModified();
    }

    /// <summary>
    /// Updates the address label
    /// </summary>
    /// <param name="label">New label for the address</param>
    public void UpdateLabel(string? label)
    {
        var trimmedLabel = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        
        if (trimmedLabel?.Length > 50)
            throw new ArgumentException("Address label cannot exceed 50 characters.", nameof(label));

        Label = trimmedLabel;
        MarkAsModified();
    }

    /// <summary>
    /// Checks if this address is suitable for shipping
    /// </summary>
    public bool CanBeUsedForShipping => Type == AddressType.Shipping || Type == AddressType.Both;

    /// <summary>
    /// Checks if this address is suitable for billing
    /// </summary>
    public bool CanBeUsedForBilling => Type == AddressType.Billing || Type == AddressType.Both;

    /// <summary>
    /// Validates if the address is in a specific country
    /// </summary>
    /// <param name="countryCode">Country code to check against</param>
    /// <returns>True if the address is in the specified country</returns>
    public bool IsInCountry(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return false;

        return Country.Equals(countryCode, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if the postal code format is valid for common countries
    /// </summary>
    public bool HasValidPostalCodeFormat()
    {
        if (string.IsNullOrWhiteSpace(PostalCode))
            return false;

        return Country.ToUpperInvariant() switch
        {
            "US" or "USA" => IsValidUSPostalCode(PostalCode),
            "CA" or "CANADA" => IsValidCanadianPostalCode(PostalCode),
            "UK" or "GB" or "UNITED KINGDOM" => IsValidUKPostalCode(PostalCode),
            _ => PostalCode.Length >= 3 && PostalCode.Length <= 10 // Generic validation
        };
    }

    private static bool IsValidUSPostalCode(string postalCode)
    {
        // US ZIP code: 12345 or 12345-6789
        return System.Text.RegularExpressions.Regex.IsMatch(postalCode, @"^\d{5}(-\d{4})?$");
    }

    private static bool IsValidCanadianPostalCode(string postalCode)
    {
        // Canadian postal code: A1A 1A1 or A1A1A1
        return System.Text.RegularExpressions.Regex.IsMatch(postalCode, @"^[A-Za-z]\d[A-Za-z]\s?\d[A-Za-z]\d$");
    }

    private static bool IsValidUKPostalCode(string postalCode)
    {
        // UK postal code: various formats like SW1A 1AA, M1 1AA, etc.
        return System.Text.RegularExpressions.Regex.IsMatch(postalCode, 
            @"^[A-Za-z]{1,2}\d[A-Za-z\d]?\s?\d[A-Za-z]{2}$");
    }

    private static void ValidateAddressCreation(string street1, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street1))
            throw new ArgumentException("Street address cannot be null or empty.", nameof(street1));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be null or empty.", nameof(state));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty.", nameof(postalCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty.", nameof(country));

        // Length validations
        if (street1.Trim().Length > 100)
            throw new ArgumentException("Street address line 1 cannot exceed 100 characters.", nameof(street1));

        if (city.Trim().Length > 50)
            throw new ArgumentException("City cannot exceed 50 characters.", nameof(city));

        if (state.Trim().Length > 50)
            throw new ArgumentException("State cannot exceed 50 characters.", nameof(state));

        if (postalCode.Trim().Length > 20)
            throw new ArgumentException("Postal code cannot exceed 20 characters.", nameof(postalCode));

        if (country.Trim().Length > 50)
            throw new ArgumentException("Country cannot exceed 50 characters.", nameof(country));
    }
}
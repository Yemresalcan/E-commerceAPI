using Nest;

namespace ECommerce.ReadModel.Models;

/// <summary>
/// Read model for Customer optimized for search and query operations
/// </summary>
[ElasticsearchType(IdProperty = nameof(Id))]
public class CustomerReadModel
{
    /// <summary>
    /// Customer unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Customer's first name
    /// </summary>
    [Text(Analyzer = "standard")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's last name
    /// </summary>
    [Text(Analyzer = "standard")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's full name for search
    /// </summary>
    [Text(Analyzer = "standard")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's email address
    /// </summary>
    [Keyword]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number
    /// </summary>
    [Keyword]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Whether the customer account is active
    /// </summary>
    [Boolean]
    public bool IsActive { get; set; }

    /// <summary>
    /// Customer registration date
    /// </summary>
    [Date]
    public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// Customer's last activity date
    /// </summary>
    [Date]
    public DateTime? LastActiveDate { get; set; }

    /// <summary>
    /// Customer addresses
    /// </summary>
    [Object]
    public List<AddressReadModel> Addresses { get; set; } = [];

    /// <summary>
    /// Customer profile information
    /// </summary>
    [Object]
    public ProfileReadModel Profile { get; set; } = new();

    /// <summary>
    /// Customer statistics
    /// </summary>
    [Object]
    public CustomerStatisticsReadModel Statistics { get; set; } = new();

    /// <summary>
    /// Customer creation date
    /// </summary>
    [Date]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Customer last update date
    /// </summary>
    [Date]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Search suggestions for autocomplete
    /// </summary>
    [Completion]
    public CompletionField Suggest { get; set; } = new();
}

/// <summary>
/// Address information for customer read model
/// </summary>
public class AddressReadModel
{
    /// <summary>
    /// Address unique identifier
    /// </summary>
    [Keyword]
    public Guid Id { get; set; }

    /// <summary>
    /// Address type (Shipping, Billing, etc.)
    /// </summary>
    [Keyword]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Street address line 1
    /// </summary>
    [Text]
    public string Street1 { get; set; } = string.Empty;

    /// <summary>
    /// Street address line 2
    /// </summary>
    [Text]
    public string? Street2 { get; set; }

    /// <summary>
    /// City name
    /// </summary>
    [Text]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State or province
    /// </summary>
    [Text]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Postal or ZIP code
    /// </summary>
    [Keyword]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country name
    /// </summary>
    [Keyword]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the primary address
    /// </summary>
    [Boolean]
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Full formatted address for search
    /// </summary>
    [Text]
    public string FullAddress { get; set; } = string.Empty;
}

/// <summary>
/// Profile information for customer read model
/// </summary>
public class ProfileReadModel
{
    /// <summary>
    /// Customer's date of birth
    /// </summary>
    [Date]
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Customer's gender
    /// </summary>
    [Keyword]
    public string? Gender { get; set; }

    /// <summary>
    /// Preferred language
    /// </summary>
    [Keyword]
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Preferred currency
    /// </summary>
    [Keyword]
    public string PreferredCurrency { get; set; } = "USD";

    /// <summary>
    /// Marketing email subscription status
    /// </summary>
    [Boolean]
    public bool MarketingEmailsEnabled { get; set; }

    /// <summary>
    /// SMS notifications subscription status
    /// </summary>
    [Boolean]
    public bool SmsNotificationsEnabled { get; set; }

    /// <summary>
    /// Customer interests/preferences
    /// </summary>
    [Keyword]
    public List<string> Interests { get; set; } = [];
}

/// <summary>
/// Customer statistics for analytics and personalization
/// </summary>
public class CustomerStatisticsReadModel
{
    /// <summary>
    /// Total number of orders placed
    /// </summary>
    [Number(NumberType.Integer)]
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total amount spent across all orders
    /// </summary>
    [Number(NumberType.Double)]
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// Currency for total spent
    /// </summary>
    [Keyword]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Average order value
    /// </summary>
    [Number(NumberType.Double)]
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Date of first order
    /// </summary>
    [Date]
    public DateTime? FirstOrderDate { get; set; }

    /// <summary>
    /// Date of last order
    /// </summary>
    [Date]
    public DateTime? LastOrderDate { get; set; }

    /// <summary>
    /// Customer lifetime value
    /// </summary>
    [Number(NumberType.Double)]
    public decimal LifetimeValue { get; set; }

    /// <summary>
    /// Customer segment (VIP, Regular, New, etc.)
    /// </summary>
    [Keyword]
    public string Segment { get; set; } = "New";
}
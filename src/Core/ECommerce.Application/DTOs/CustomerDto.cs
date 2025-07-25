namespace ECommerce.Application.DTOs;

/// <summary>
/// Data transfer object for customer information
/// </summary>
public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegistrationDate,
    DateTime? LastActiveDate,
    List<AddressDto> Addresses,
    ProfileDto Profile,
    CustomerStatisticsDto Statistics,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// Data transfer object for address information
/// </summary>
public record AddressDto(
    Guid Id,
    string Type,
    string Street1,
    string? Street2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsPrimary,
    string FullAddress
);

/// <summary>
/// Data transfer object for profile information
/// </summary>
public record ProfileDto(
    DateTime? DateOfBirth,
    string? Gender,
    string PreferredLanguage,
    string PreferredCurrency,
    bool MarketingEmailsEnabled,
    bool SmsNotificationsEnabled,
    List<string> Interests
);

/// <summary>
/// Data transfer object for customer statistics
/// </summary>
public record CustomerStatisticsDto(
    int TotalOrders,
    decimal TotalSpent,
    string Currency,
    decimal AverageOrderValue,
    DateTime? FirstOrderDate,
    DateTime? LastOrderDate,
    decimal LifetimeValue,
    string Segment
);
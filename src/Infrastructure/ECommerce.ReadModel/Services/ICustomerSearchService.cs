using ECommerce.ReadModel.Models;
using Nest;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Interface for customer search operations
/// </summary>
public interface ICustomerSearchService : IElasticsearchService<CustomerReadModel>
{
    /// <summary>
    /// Searches customers with advanced filtering
    /// </summary>
    Task<CustomerSearchResult> SearchCustomersAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customer suggestions for autocomplete
    /// </summary>
    Task<IEnumerable<string>> GetCustomerSuggestionsAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers by segment
    /// </summary>
    Task<ISearchResponse<CustomerReadModel>> GetCustomersBySegmentAsync(string segment, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers by location (country/state/city)
    /// </summary>
    Task<ISearchResponse<CustomerReadModel>> GetCustomersByLocationAsync(string? country = null, string? state = null, string? city = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets high-value customers
    /// </summary>
    Task<ISearchResponse<CustomerReadModel>> GetHighValueCustomersAsync(decimal minLifetimeValue, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently registered customers
    /// </summary>
    Task<ISearchResponse<CustomerReadModel>> GetRecentCustomersAsync(int days = 30, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inactive customers
    /// </summary>
    Task<ISearchResponse<CustomerReadModel>> GetInactiveCustomersAsync(int daysSinceLastActivity = 90, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customer analytics and statistics
    /// </summary>
    Task<CustomerAnalytics> GetCustomerAnalyticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Customer search request parameters
/// </summary>
public class CustomerSearchRequest
{
    public string? Query { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsActive { get; set; }
    public string? Segment { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public decimal? MinLifetimeValue { get; set; }
    public decimal? MaxLifetimeValue { get; set; }
    public int? MinOrders { get; set; }
    public int? MaxOrders { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? SortBy { get; set; } = "registration_desc"; // registration_desc, registration_asc, name_asc, name_desc, lifetime_value_desc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Customer search result with aggregations
/// </summary>
public class CustomerSearchResult
{
    public IEnumerable<CustomerReadModel> Customers { get; set; } = [];
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public CustomerSearchAggregations Aggregations { get; set; } = new();
}

/// <summary>
/// Customer search aggregations
/// </summary>
public class CustomerSearchAggregations
{
    public Dictionary<string, long> Segments { get; set; } = new();
    public Dictionary<string, long> Countries { get; set; } = new();
    public Dictionary<string, long> PreferredLanguages { get; set; } = new();
    public Dictionary<string, long> CustomerValueRanges { get; set; } = new();
    public long ActiveCustomers { get; set; }
    public double AverageLifetimeValue { get; set; }
    public double TotalCustomerValue { get; set; }
}

/// <summary>
/// Customer analytics data
/// </summary>
public class CustomerAnalytics
{
    public long TotalCustomers { get; set; }
    public long ActiveCustomers { get; set; }
    public long InactiveCustomers { get; set; }
    public double AverageLifetimeValue { get; set; }
    public double TotalCustomerValue { get; set; }
    public Dictionary<string, long> SegmentDistribution { get; set; } = new();
    public Dictionary<string, long> CountryDistribution { get; set; } = new();
    public Dictionary<string, long> LanguageDistribution { get; set; } = new();
    public Dictionary<DateTime, long> RegistrationsOverTime { get; set; } = new();
    public Dictionary<string, long> CustomerValueRanges { get; set; } = new();
}
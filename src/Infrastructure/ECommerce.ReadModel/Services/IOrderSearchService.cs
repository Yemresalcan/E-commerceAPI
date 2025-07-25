using ECommerce.ReadModel.Models;
using Nest;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Interface for order search operations
/// </summary>
public interface IOrderSearchService : IElasticsearchService<OrderReadModel>
{
    /// <summary>
    /// Searches orders with advanced filtering
    /// </summary>
    Task<OrderSearchResult> SearchOrdersAsync(OrderSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by customer ID
    /// </summary>
    Task<ISearchResponse<OrderReadModel>> GetOrdersByCustomerAsync(Guid customerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by status
    /// </summary>
    Task<ISearchResponse<OrderReadModel>> GetOrdersByStatusAsync(string status, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    Task<ISearchResponse<OrderReadModel>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets order analytics and statistics
    /// </summary>
    Task<OrderAnalytics> GetOrderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent orders
    /// </summary>
    Task<ISearchResponse<OrderReadModel>> GetRecentOrdersAsync(int count = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Order search request parameters
/// </summary>
public class OrderSearchRequest
{
    public string? Query { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Status { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string? SortBy { get; set; } = "created_desc"; // created_desc, created_asc, amount_desc, amount_asc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Order search result with aggregations
/// </summary>
public class OrderSearchResult
{
    public IEnumerable<OrderReadModel> Orders { get; set; } = [];
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public OrderSearchAggregations Aggregations { get; set; } = new();
}

/// <summary>
/// Order search aggregations
/// </summary>
public class OrderSearchAggregations
{
    public Dictionary<string, long> StatusDistribution { get; set; } = new();
    public Dictionary<string, long> PaymentMethods { get; set; } = new();
    public Dictionary<string, long> OrderValueRanges { get; set; } = new();
    public double TotalRevenue { get; set; }
    public double AverageOrderValue { get; set; }
}

/// <summary>
/// Order analytics data
/// </summary>
public class OrderAnalytics
{
    public long TotalOrders { get; set; }
    public double TotalRevenue { get; set; }
    public double AverageOrderValue { get; set; }
    public Dictionary<string, long> StatusDistribution { get; set; } = new();
    public Dictionary<string, long> PaymentMethodDistribution { get; set; } = new();
    public Dictionary<DateTime, long> OrdersOverTime { get; set; } = new();
    public Dictionary<string, long> OrderValueRanges { get; set; } = new();
}
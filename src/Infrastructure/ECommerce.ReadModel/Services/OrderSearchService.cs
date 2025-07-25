using ECommerce.ReadModel.Configuration;
using ECommerce.ReadModel.Configurations;
using ECommerce.ReadModel.Models;
using Microsoft.Extensions.Options;
using Nest;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Implementation of order search operations using Elasticsearch
/// </summary>
public class OrderSearchService : BaseElasticsearchService<OrderReadModel>, IOrderSearchService
{
    protected override string IndexName => OrderIndexConfiguration.IndexName;

    public OrderSearchService(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> settings,
        ILogger<OrderSearchService> logger)
        : base(elasticClient, settings, logger)
    {
    }

    public override async Task<bool> CreateIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var indexName = GetIndexName();
            var indexMapping = OrderIndexConfiguration.GetIndexMapping(indexName);

            var response = await _elasticClient.Indices.CreateAsync(indexMapping, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to create order index {IndexName}: {Error}", indexName, response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogInformation("Successfully created order index: {IndexName}", indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating order index");
            return false;
        }
    }

    public override async Task<ISearchResponse<OrderReadModel>> SimpleSearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(query)
                    .Fields(f => f
                        .Field(o => o.Id)
                        .Field(o => o.Customer.FullName)
                        .Field(o => o.Customer.Email)
                        .Field(o => o.Status)
                    )
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(Fuzziness.Auto)
                )
            )
            .Sort(s => s
                .Descending(o => o.CreatedAt)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public override async Task<ISearchResponse<OrderReadModel>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .Size(5)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Customer.FullName)
                    .Query(query)
                )
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<OrderSearchResult> SearchOrdersAsync(OrderSearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize)
            .Query(q => BuildOrderQuery(q, request))
            .Sort(s => BuildOrderSort(s, request.SortBy))
            .Aggregations(a => OrderIndexConfiguration.GetAggregations());

        var response = await SearchAsync(searchDescriptor, cancellationToken);

        return new OrderSearchResult
        {
            Orders = response.Documents,
            TotalCount = response.Total,
            Page = request.Page,
            PageSize = request.PageSize,
            Aggregations = ExtractOrderAggregations(response.Aggregations)
        };
    }

    public async Task<ISearchResponse<OrderReadModel>> GetOrdersByCustomerAsync(Guid customerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Term(t => t.CustomerId, customerId)
            )
            .Sort(s => s
                .Descending(o => o.CreatedAt)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<OrderReadModel>> GetOrdersByStatusAsync(string status, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Term(t => t.Status, status)
            )
            .Sort(s => s
                .Descending(o => o.CreatedAt)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<OrderReadModel>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .DateRange(dr => dr
                    .Field(f => f.CreatedAt)
                    .GreaterThanOrEquals(startDate)
                    .LessThanOrEquals(endDate)
                )
            )
            .Sort(s => s
                .Descending(o => o.CreatedAt)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<OrderAnalytics> GetOrderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .Size(0) // We only want aggregations
            .Query(q => BuildDateRangeQuery(q, startDate, endDate))
            .Aggregations(a => OrderIndexConfiguration.GetAggregations());

        var response = await SearchAsync(searchDescriptor, cancellationToken);

        return ExtractOrderAnalytics(response.Aggregations, response.Total);
    }

    public async Task<ISearchResponse<OrderReadModel>> GetRecentOrdersAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<OrderReadModel>()
            .Index(GetIndexName())
            .Size(count)
            .Sort(s => s
                .Descending(o => o.CreatedAt)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    private QueryContainer BuildOrderQuery(QueryContainerDescriptor<OrderReadModel> q, OrderSearchRequest request)
    {
        var queries = new List<QueryContainer>();
        var filters = new List<QueryContainer>();

        // Text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queries.Add(q.MultiMatch(mm => mm
                .Query(request.Query)
                .Fields(f => f
                    .Field(o => o.Id)
                    .Field(o => o.Customer.FullName)
                    .Field(o => o.Customer.Email)
                    .Field(o => o.Status)
                )
                .Type(TextQueryType.BestFields)
                .Fuzziness(Fuzziness.Auto)
            ));
        }

        // Customer filter
        if (request.CustomerId.HasValue)
        {
            filters.Add(q.Term(t => t.CustomerId, request.CustomerId.Value));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            filters.Add(q.Term(t => t.Status, request.Status));
        }

        // Amount range filter
        if (request.MinAmount.HasValue || request.MaxAmount.HasValue)
        {
            filters.Add(q.Range(r => r
                .Field(f => f.TotalAmount)
                .GreaterThanOrEquals((double?)request.MinAmount)
                .LessThanOrEquals((double?)request.MaxAmount)
            ));
        }

        // Date range filter
        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            filters.Add(q.DateRange(dr => dr
                .Field(f => f.CreatedAt)
                .GreaterThanOrEquals(request.StartDate)
                .LessThanOrEquals(request.EndDate)
            ));
        }

        // Payment method filter
        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            filters.Add(q.Term(t => t.Payment!.Method, request.PaymentMethod));
        }

        // Payment status filter
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
        {
            filters.Add(q.Term(t => t.Payment!.Status, request.PaymentStatus));
        }

        return q.Bool(b => b
            .Must(queries.ToArray())
            .Filter(filters.ToArray())
        );
    }

    private SortDescriptor<OrderReadModel> BuildOrderSort(SortDescriptor<OrderReadModel> s, string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "created_asc" => s.Ascending(o => o.CreatedAt),
            "amount_desc" => s.Descending(o => o.TotalAmount),
            "amount_asc" => s.Ascending(o => o.TotalAmount),
            _ => s.Descending(o => o.CreatedAt)
        };
    }

    private QueryContainer BuildDateRangeQuery(QueryContainerDescriptor<OrderReadModel> q, DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue || endDate.HasValue)
        {
            return q.DateRange(dr => dr
                .Field(f => f.CreatedAt)
                .GreaterThanOrEquals(startDate)
                .LessThanOrEquals(endDate)
            );
        }

        return q.MatchAll();
    }

    private OrderSearchAggregations ExtractOrderAggregations(AggregateDictionary aggregations)
    {
        var aggs = new OrderSearchAggregations();

        if (aggregations.Terms("status_distribution") is { } statusAgg)
        {
            aggs.StatusDistribution = statusAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0L);
        }

        if (aggregations.Terms("payment_methods") is { } paymentAgg)
        {
            aggs.PaymentMethods = paymentAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0L);
        }

        if (aggregations.Range("order_value_ranges") is { } valueRangesAgg)
        {
            aggs.OrderValueRanges = valueRangesAgg.Buckets.ToDictionary(b => b.Key, b => b.DocCount);
        }

        if (aggregations.Sum("total_revenue") is { } revenueAgg)
        {
            aggs.TotalRevenue = revenueAgg.Value ?? 0;
        }

        if (aggregations.Average("average_order_value") is { } avgAgg)
        {
            aggs.AverageOrderValue = avgAgg.Value ?? 0;
        }

        return aggs;
    }

    private OrderAnalytics ExtractOrderAnalytics(AggregateDictionary aggregations, long totalOrders)
    {
        var analytics = new OrderAnalytics
        {
            TotalOrders = totalOrders
        };

        if (aggregations.Terms("status_distribution") is { } statusAgg)
        {
            analytics.StatusDistribution = statusAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0  );
        }

        if (aggregations.Terms("payment_methods") is { } paymentAgg)
        {
            analytics.PaymentMethodDistribution = paymentAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0  );
        }

        if (aggregations.DateHistogram("orders_over_time") is { } dateHistAgg)
        {
            analytics.OrdersOverTime = dateHistAgg.Buckets.ToDictionary(
                b => b.Date,
                b =>  b.DocCount ?? 0  
            );
        }

        if (aggregations.Range("order_value_ranges") is { } valueRangesAgg)
        {
            analytics.OrderValueRanges = valueRangesAgg.Buckets.ToDictionary(b => b.Key, b => b.DocCount);        
        }

        if (aggregations.Sum("total_revenue") is { } revenueAgg)
        {
            analytics.TotalRevenue = revenueAgg.Value ?? 0;
        }

        if (aggregations.Average("average_order_value") is { } avgAgg)
        {
            analytics.AverageOrderValue = avgAgg.Value ?? 0;
        }

        return analytics;
    }
}
using ECommerce.ReadModel.Configuration;
using ECommerce.ReadModel.Configurations;
using ECommerce.ReadModel.Models;
using Microsoft.Extensions.Options;
using Nest;

namespace ECommerce.ReadModel.Services;

/// <summary>
/// Implementation of customer search operations using Elasticsearch
/// </summary>
public class CustomerSearchService : BaseElasticsearchService<CustomerReadModel>, ICustomerSearchService
{
    protected override string IndexName => CustomerIndexConfiguration.IndexName;

    public CustomerSearchService(
        IElasticClient elasticClient,
        IOptions<ElasticsearchSettings> settings,
        ILogger<CustomerSearchService> logger)
        : base(elasticClient, settings, logger)
    {
    }

    public override async Task<bool> CreateIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var indexName = GetIndexName();
            var indexMapping = CustomerIndexConfiguration.GetIndexMapping(indexName);

            var response = await _elasticClient.Indices.CreateAsync(indexMapping, cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to create customer index {IndexName}: {Error}", indexName, response.OriginalException?.Message ?? response.ServerError?.ToString());
                return false;
            }

            _logger.LogInformation("Successfully created customer index: {IndexName}", indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating customer index");
            return false;
        }
    }

    public override async Task<ISearchResponse<CustomerReadModel>> SimpleSearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(query)
                    .Fields(f => f
                        .Field(c => c.FullName, 2.0)
                        .Field(c => c.FirstName)
                        .Field(c => c.LastName)
                        .Field(c => c.Email, 1.5)
                        .Field(c => c.PhoneNumber)
                    )
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(Fuzziness.Auto)
                )
            )
            .Sort(s => s
                .Descending(SortSpecialField.Score)
                .Descending(c => c.RegistrationDate)
            )
            .Highlight(h => h
                .Fields(f => f
                    .Field(c => c.FullName)
                    .Field(c => c.Email)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public override async Task<ISearchResponse<CustomerReadModel>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .Size(10)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.FullName.Suffix("autocomplete"))
                    .Query(query)
                )
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<CustomerSearchResult> SearchCustomersAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize)
            .Query(q => BuildCustomerQuery(q, request))
            .Sort(s => BuildCustomerSort(s, request.SortBy))
            .Aggregations(a => CustomerIndexConfiguration.GetAggregations())
            .Highlight(h => h
                .Fields(f => f
                    .Field(c => c.FullName)
                    .Field(c => c.Email)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
            );

        var response = await SearchAsync(searchDescriptor, cancellationToken);

        return new CustomerSearchResult
        {
            Customers = response.Documents,
            TotalCount = response.Total,
            Page = request.Page,
            PageSize = request.PageSize,
            Aggregations = ExtractCustomerAggregations(response.Aggregations)
        };
    }

    public async Task<IEnumerable<string>> GetCustomerSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchResponse = await GetSuggestionsAsync(query, cancellationToken);
        
        return searchResponse.Documents
            .Select(c => c.FullName)
            .Distinct()
            .Take(10);
    }

    public async Task<ISearchResponse<CustomerReadModel>> GetCustomersBySegmentAsync(string segment, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Term(t => t.Statistics.Segment, segment)
            )
            .Sort(s => s
                .Descending(c => c.Statistics.LifetimeValue)
                .Descending(c => c.RegistrationDate)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<CustomerReadModel>> GetCustomersByLocationAsync(string? country = null, string? state = null, string? city = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => BuildLocationQuery(q, country, state, city))
            .Sort(s => s
                .Descending(c => c.RegistrationDate)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<CustomerReadModel>> GetHighValueCustomersAsync(decimal minLifetimeValue, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Range(r => r
                    .Field(f => f.Statistics.LifetimeValue)
                    .GreaterThanOrEquals((double)minLifetimeValue)
                )
            )
            .Sort(s => s
                .Descending(c => c.Statistics.LifetimeValue)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<CustomerReadModel>> GetRecentCustomersAsync(int days = 30, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .DateRange(dr => dr
                    .Field(f => f.RegistrationDate)
                    .GreaterThanOrEquals(cutoffDate)
                )
            )
            .Sort(s => s
                .Descending(c => c.RegistrationDate)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<ISearchResponse<CustomerReadModel>> GetInactiveCustomersAsync(int daysSinceLastActivity = 90, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastActivity);

        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .DateRange(dr => dr
                            .Field(ff => ff.LastActiveDate)
                            .LessThan(cutoffDate)
                        )
                    )
                )
            )
            .Sort(s => s
                .Ascending(c => c.LastActiveDate)
            );

        return await SearchAsync(searchDescriptor, cancellationToken);
    }

    public async Task<CustomerAnalytics> GetCustomerAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var searchDescriptor = new SearchDescriptor<CustomerReadModel>()
            .Index(GetIndexName())
            .Size(0) // We only want aggregations
            .Aggregations(a => CustomerIndexConfiguration.GetAggregations());

        var response = await SearchAsync(searchDescriptor, cancellationToken);

        return ExtractCustomerAnalytics(response.Aggregations, response.Total);
    }

    private QueryContainer BuildCustomerQuery(QueryContainerDescriptor<CustomerReadModel> q, CustomerSearchRequest request)
    {
        var queries = new List<QueryContainer>();
        var filters = new List<QueryContainer>();

        // Text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queries.Add(q.MultiMatch(mm => mm
                .Query(request.Query)
                .Fields(f => f
                    .Field(c => c.FullName, 2.0)
                    .Field(c => c.FirstName)
                    .Field(c => c.LastName)
                    .Field(c => c.Email, 1.5)
                    .Field(c => c.PhoneNumber)
                )
                .Type(TextQueryType.BestFields)
                .Fuzziness(Fuzziness.Auto)
            ));
        }

        // Email filter
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            filters.Add(q.Term(t => t.Email, request.Email));
        }

        // Phone number filter
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            filters.Add(q.Term(t => t.PhoneNumber, request.PhoneNumber));
        }

        // Active status filter
        if (request.IsActive.HasValue)
        {
            filters.Add(q.Term(t => t.IsActive, request.IsActive.Value));
        }

        // Segment filter
        if (!string.IsNullOrWhiteSpace(request.Segment))
        {
            filters.Add(q.Term(t => t.Statistics.Segment, request.Segment));
        }

        // Location filters
        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            filters.Add(q.Term(t => t.Addresses.First().Country, request.Country));
        }

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            filters.Add(q.Term(t => t.Addresses.First().State, request.State));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            filters.Add(q.Term(t => t.Addresses.First().City, request.City));
        }

        // Registration date range filter
        if (request.RegistrationStartDate.HasValue || request.RegistrationEndDate.HasValue)
        {
            filters.Add(q.DateRange(dr => dr
                .Field(f => f.RegistrationDate)
                .GreaterThanOrEquals(request.RegistrationStartDate)
                .LessThanOrEquals(request.RegistrationEndDate)
            ));
        }

        // Lifetime value range filter
        if (request.MinLifetimeValue.HasValue || request.MaxLifetimeValue.HasValue)
        {
            filters.Add(q.Range(r => r
                .Field(f => f.Statistics.LifetimeValue)
                .GreaterThanOrEquals((double?)request.MinLifetimeValue)
                .LessThanOrEquals((double?)request.MaxLifetimeValue)
            ));
        }

        // Orders count range filter
        if (request.MinOrders.HasValue || request.MaxOrders.HasValue)
        {
            filters.Add(q.Range(r => r
                .Field(f => f.Statistics.TotalOrders)
                .GreaterThanOrEquals(request.MinOrders)
                .LessThanOrEquals(request.MaxOrders)
            ));
        }

        // Preferred language filter
        if (!string.IsNullOrWhiteSpace(request.PreferredLanguage))
        {
            filters.Add(q.Term(t => t.Profile.PreferredLanguage, request.PreferredLanguage));
        }

        return q.Bool(b => b
            .Must(queries.ToArray())
            .Filter(filters.ToArray())
        );
    }

    private QueryContainer BuildLocationQuery(QueryContainerDescriptor<CustomerReadModel> q, string? country, string? state, string? city)
    {
        var filters = new List<QueryContainer>();

        if (!string.IsNullOrWhiteSpace(country))
        {
            filters.Add(q.Term(t => t.Addresses.First().Country, country));
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            filters.Add(q.Term(t => t.Addresses.First().State, state));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            filters.Add(q.Term(t => t.Addresses.First().City, city));
        }

        return filters.Any() ? q.Bool(b => b.Filter(filters.ToArray())) : q.MatchAll();
    }

    private SortDescriptor<CustomerReadModel> BuildCustomerSort(SortDescriptor<CustomerReadModel> s, string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "registration_asc" => s.Ascending(c => c.RegistrationDate),
            "name_asc" => s.Ascending(c => c.FullName.Suffix("keyword")),
            "name_desc" => s.Descending(c => c.FullName.Suffix("keyword")),
            "lifetime_value_desc" => s.Descending(c => c.Statistics.LifetimeValue),
            _ => s.Descending(c => c.RegistrationDate)
        };
    }

    private CustomerSearchAggregations ExtractCustomerAggregations(AggregateDictionary aggregations)
    {
        var aggs = new CustomerSearchAggregations();

        if (aggregations.Terms("customer_segments") is { } segmentsAgg)
        {
            aggs.Segments = segmentsAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0  );
        }

        if (aggregations.Terms("countries") is { } countriesAgg)
        {
            aggs.Countries = countriesAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0 );
        }

        if (aggregations.Terms("preferred_languages") is { } languagesAgg)
        {
            aggs.PreferredLanguages = languagesAgg.Buckets.ToDictionary(b => b.Key, b =>  b.DocCount ?? 0 );
        }

        if (aggregations.Range("customer_value_ranges") is { } valueRangesAgg)
        {
            aggs.CustomerValueRanges = valueRangesAgg.Buckets.ToDictionary(b => b.Key, b => b.DocCount);        }

        if (aggregations.Filter("active_customers") is { } activeAgg)
        {
            aggs.ActiveCustomers = activeAgg.DocCount;
        }

        if (aggregations.Average("avg_lifetime_value") is { } avgLifetimeAgg)
        {
            aggs.AverageLifetimeValue = avgLifetimeAgg.Value ?? 0;
        }

        if (aggregations.Sum("total_customer_value") is { } totalValueAgg)
        {
            aggs.TotalCustomerValue = totalValueAgg.Value ?? 0;
        }

        return aggs;
    }

    private CustomerAnalytics ExtractCustomerAnalytics(AggregateDictionary aggregations, long totalCustomers)
    {
        var analytics = new CustomerAnalytics
        {
            TotalCustomers = totalCustomers
        };

        if (aggregations.Terms("customer_segments") is { } segmentsAgg)
        {
            analytics.SegmentDistribution = segmentsAgg.Buckets.ToDictionary(b => b.Key, b => (long)(b.DocCount ?? 0));
        }

        if (aggregations.Terms("countries") is { } countriesAgg)
        {
            analytics.CountryDistribution = countriesAgg.Buckets.ToDictionary(b => b.Key, b => (long)(b.DocCount ?? 0));
        }

        if (aggregations.Terms("preferred_languages") is { } languagesAgg)
        {
            analytics.LanguageDistribution = languagesAgg.Buckets.ToDictionary(b => b.Key, b => (long)(b.DocCount ?? 0));
        }

        if (aggregations.DateHistogram("registrations_over_time") is { } dateHistAgg)
        {
            analytics.RegistrationsOverTime = dateHistAgg.Buckets.ToDictionary(
                b => b.Date,
                b =>  b.DocCount ?? 0  
            );
        }

        if (aggregations.Range("customer_value_ranges") is { } valueRangesAgg)
        {
            analytics.CustomerValueRanges = valueRangesAgg.Buckets.ToDictionary(b => b.Key, b => b.DocCount);      
        }

        if (aggregations.Filter("active_customers") is { } activeAgg)
        {
            analytics.ActiveCustomers = activeAgg.DocCount  ;
            analytics.InactiveCustomers = totalCustomers - analytics.ActiveCustomers;
        }

        if (aggregations.Average("avg_lifetime_value") is { } avgLifetimeAgg)
        {
            analytics.AverageLifetimeValue = avgLifetimeAgg.Value ?? 0;
        }

        if (aggregations.Sum("total_customer_value") is { } totalValueAgg)
        {
            analytics.TotalCustomerValue = totalValueAgg.Value ?? 0;
        }

        return analytics;
    }
}
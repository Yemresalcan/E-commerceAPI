using ECommerce.ReadModel.Models;
using Nest;

namespace ECommerce.ReadModel.Configurations;

/// <summary>
/// Elasticsearch index configuration for Customer read model
/// </summary>
public static class CustomerIndexConfiguration
{
    public const string IndexName = "customers";

    /// <summary>
    /// Creates the index mapping for customers
    /// </summary>
    public static CreateIndexDescriptor GetIndexMapping(string indexName)
    {
        return new CreateIndexDescriptor(indexName)
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(1)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Standard("standard_analyzer", sa => sa
                            .StopWords("_english_")
                        )
                        .Custom("name_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "stop")
                        )
                        .Custom("autocomplete_analyzer", ca => ca
                            .Tokenizer("autocomplete_tokenizer")
                            .Filters("lowercase")
                        )
                        .Custom("autocomplete_search_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase")
                        )
                    )
                    .Tokenizers(t => t
                        .EdgeNGram("autocomplete_tokenizer", en => en
                            .MinGram(1)
                            .MaxGram(20)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit)
                        )
                    )
                )
            )
            .Map<CustomerReadModel>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t
                        .Name(n => n.FirstName)
                        .Analyzer("name_analyzer")
                        .Fields(f => f
                            .Keyword(kk => kk.Name("keyword"))
                        )
                    )
                    .Text(t => t
                        .Name(n => n.LastName)
                        .Analyzer("name_analyzer")
                        .Fields(f => f
                            .Keyword(kk => kk.Name("keyword"))
                        )
                    )
                    .Text(t => t
                        .Name(n => n.FullName)
                        .Analyzer("name_analyzer")
                        .Fields(f => f
                            .Text(tt => tt
                                .Name("autocomplete")
                                .Analyzer("autocomplete_analyzer")
                                .SearchAnalyzer("autocomplete_search_analyzer")
                            )
                            .Keyword(kk => kk.Name("keyword"))
                        )
                    )
                    .Keyword(k => k.Name(n => n.Email))
                    .Keyword(k => k.Name(n => n.PhoneNumber))
                    .Boolean(b => b.Name(n => n.IsActive))
                    .Date(d => d.Name(n => n.RegistrationDate))
                    .Date(d => d.Name(n => n.LastActiveDate))
                    .Object<AddressReadModel>(o => o
                        .Name(n => n.Addresses)
                        .Properties(ap => ap
                            .Keyword(k => k.Name(a => a.Id))
                            .Keyword(k => k.Name(a => a.Type))
                            .Text(t => t.Name(a => a.Street1))
                            .Text(t => t.Name(a => a.Street2))
                            .Text(t => t.Name(a => a.City))
                            .Text(t => t.Name(a => a.State))
                            .Keyword(k => k.Name(a => a.PostalCode))
                            .Keyword(k => k.Name(a => a.Country))
                            .Boolean(b => b.Name(a => a.IsPrimary))
                            .Text(t => t.Name(a => a.FullAddress))
                        )
                    )
                    .Object<ProfileReadModel>(o => o
                        .Name(n => n.Profile)
                        .Properties(pp => pp
                            .Date(d => d.Name(pr => pr.DateOfBirth))
                            .Keyword(k => k.Name(pr => pr.Gender))
                            .Keyword(k => k.Name(pr => pr.PreferredLanguage))
                            .Keyword(k => k.Name(pr => pr.PreferredCurrency))
                            .Boolean(b => b.Name(pr => pr.MarketingEmailsEnabled))
                            .Boolean(b => b.Name(pr => pr.SmsNotificationsEnabled))
                            .Keyword(k => k.Name(pr => pr.Interests))
                        )
                    )
                    .Object<CustomerStatisticsReadModel>(o => o
                        .Name(n => n.Statistics)
                        .Properties(sp => sp
                            .Number(n => n
                                .Name(st => st.TotalOrders)
                                .Type(NumberType.Integer)
                            )
                            .Number(n => n
                                .Name(st => st.TotalSpent)
                                .Type(NumberType.Double)
                            )
                            .Keyword(k => k.Name(st => st.Currency))
                            .Number(n => n
                                .Name(st => st.AverageOrderValue)
                                .Type(NumberType.Double)
                            )
                            .Date(d => d.Name(st => st.FirstOrderDate))
                            .Date(d => d.Name(st => st.LastOrderDate))
                            .Number(n => n
                                .Name(st => st.LifetimeValue)
                                .Type(NumberType.Double)
                            )
                            .Keyword(k => k.Name(st => st.Segment))
                        )
                    )
                    .Date(d => d.Name(n => n.CreatedAt))
                    .Date(d => d.Name(n => n.UpdatedAt))
                    .Completion(c => c
                        .Name(n => n.Suggest)
                        .Contexts(ctx => ctx
                            .Category(cat => cat
                                .Name("segment")
                                .Path(p => p.Statistics.Segment)
                            )
                        )
                    )
                )
            );
    }

    /// <summary>
    /// Gets search settings for customer queries
    /// </summary>
    public static SearchDescriptor<CustomerReadModel> GetSearchDescriptor(string indexName)
    {
        return new SearchDescriptor<CustomerReadModel>()
            .Index(indexName)
            .Size(20)
            .Source(s => s.IncludeAll())
            .Highlight(h => h
                .Fields(f => f
                    .Field(c => c.FullName)
                    .Field(c => c.Email)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
            )
            .Sort(s => s
                .Descending(f => f.RegistrationDate)
            );
    }

    /// <summary>
    /// Creates aggregations for customer analytics
    /// </summary>
    public static AggregationContainerDescriptor<CustomerReadModel> GetAggregations()
    {
        return new AggregationContainerDescriptor<CustomerReadModel>()
            .Terms("customer_segments", t => t
                .Field(f => f.Statistics.Segment)
                .Size(10)
            )
            .DateHistogram("registrations_over_time", d => d
                .Field(f => f.RegistrationDate)
                .CalendarInterval(DateInterval.Month)
                .MinimumDocumentCount(0)
            )
            .Terms("countries", t => t
                .Field(f => f.Addresses.First().Country)
                .Size(20)
            )
            .Terms("preferred_languages", t => t
                .Field(f => f.Profile.PreferredLanguage)
                .Size(10)
            )
            .Filter("active_customers", f => f
                .Filter(ff => ff.Term(t => t.IsActive, true))
            )
            .Range("customer_value_ranges", r => r
                .Field(f => f.Statistics.TotalSpent)
                .Ranges(
                    rng => rng.To(100),
                    rng => rng.From(100).To(500),
                    rng => rng.From(500).To(1000),
                    rng => rng.From(1000).To(5000),
                    rng => rng.From(5000)
                )
            )
            .Average("avg_lifetime_value", a => a
                .Field(f => f.Statistics.LifetimeValue)
            )
            .Sum("total_customer_value", s => s
                .Field(f => f.Statistics.TotalSpent)
            );
    }
}
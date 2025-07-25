using ECommerce.ReadModel.Models;
using Nest;

namespace ECommerce.ReadModel.Configurations;

/// <summary>
/// Elasticsearch index configuration for Order read model
/// </summary>
public static class OrderIndexConfiguration
{
    public const string IndexName = "orders";

    /// <summary>
    /// Creates the index mapping for orders
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
                    )
                )
            )
            .Map<OrderReadModel>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Keyword(k => k.Name(n => n.CustomerId))
                    .Object<CustomerSummaryReadModel>(o => o
                        .Name(n => n.Customer)
                        .Properties(cp => cp
                            .Keyword(k => k.Name(cn => cn.Id))
                            .Text(t => t
                                .Name(cn => cn.FullName)
                                .Analyzer("standard_analyzer")
                            )
                            .Keyword(k => k.Name(cn => cn.Email))
                            .Keyword(k => k.Name(cn => cn.PhoneNumber))
                        )
                    )
                    .Keyword(k => k.Name(n => n.Status))
                    .Text(t => t.Name(n => n.ShippingAddress))
                    .Text(t => t.Name(n => n.BillingAddress))
                    .Object<OrderItemReadModel>(o => o
                        .Name(n => n.Items)
                        .Properties(ip => ip
                            .Keyword(k => k.Name(i => i.Id))
                            .Keyword(k => k.Name(i => i.ProductId))
                            .Text(t => t.Name(i => i.ProductName))
                            .Keyword(k => k.Name(i => i.ProductSku))
                            .Number(n => n
                                .Name(i => i.Quantity)
                                .Type(NumberType.Integer)
                            )
                            .Number(n => n
                                .Name(i => i.UnitPrice)
                                .Type(NumberType.Double)
                            )
                            .Keyword(k => k.Name(i => i.Currency))
                            .Number(n => n
                                .Name(i => i.TotalPrice)
                                .Type(NumberType.Double)
                            )
                        )
                    )
                    .Object<PaymentReadModel>(o => o
                        .Name(n => n.Payment)
                        .Properties(pp => pp
                            .Keyword(k => k.Name(pn => pn.Id))
                            .Keyword(k => k.Name(pn => pn.Method))
                            .Keyword(k => k.Name(pn => pn.Status))
                            .Number(n => n
                                .Name(pn => pn.Amount)
                                .Type(NumberType.Double)
                            )
                            .Keyword(k => k.Name(pn => pn.Currency))
                            .Keyword(k => k.Name(pn => pn.TransactionReference))
                            .Date(d => d.Name(pn => pn.ProcessedAt))
                        )
                    )
                    .Number(n => n
                        .Name(nn => nn.TotalAmount)
                        .Type(NumberType.Double)
                    )
                    .Keyword(k => k.Name(n => n.Currency))
                    .Number(n => n
                        .Name(nn => nn.TotalItemCount)
                        .Type(NumberType.Integer)
                    )
                    .Date(d => d.Name(n => n.CreatedAt))
                    .Date(d => d.Name(n => n.UpdatedAt))
                    .Date(d => d.Name(n => n.ConfirmedAt))
                    .Date(d => d.Name(n => n.ShippedAt))
                    .Date(d => d.Name(n => n.DeliveredAt))
                    .Date(d => d.Name(n => n.CancelledAt))
                    .Text(t => t.Name(n => n.CancellationReason))
                )
            );
    }

    /// <summary>
    /// Gets search settings for order queries
    /// </summary>
    public static SearchDescriptor<OrderReadModel> GetSearchDescriptor(string indexName)
    {
        return new SearchDescriptor<OrderReadModel>()
            .Index(indexName)
            .Size(20)
            .Source(s => s.IncludeAll())
            .Sort(s => s
                .Descending(f => f.CreatedAt)
            );
    }

    /// <summary>
    /// Creates aggregations for order analytics
    /// </summary>
    public static AggregationContainerDescriptor<OrderReadModel> GetAggregations()
    {
        return new AggregationContainerDescriptor<OrderReadModel>()
            .Terms("status_distribution", t => t
                .Field(f => f.Status)
                .Size(10)
            )
            .DateHistogram("orders_over_time", d => d
                .Field(f => f.CreatedAt)
                .CalendarInterval(DateInterval.Day)
                .MinimumDocumentCount(0)
            )
            .Range("order_value_ranges", r => r
                .Field(f => f.TotalAmount)
                .Ranges(
                    rng => rng.To(100),
                    rng => rng.From(100).To(500),
                    rng => rng.From(500).To(1000),
                    rng => rng.From(1000)
                )
            )
            .Terms("payment_methods", t => t
                .Field("payment.method")
                .Size(10)
            )
            .Sum("total_revenue", s => s
                .Field(f => f.TotalAmount)
            )
            .Average("average_order_value", a => a
                .Field(f => f.TotalAmount)
            );
    }
}
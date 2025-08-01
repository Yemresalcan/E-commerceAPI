using ECommerce.ReadModel.Models;
using Nest;

namespace ECommerce.ReadModel.Configurations;

/// <summary>
/// Elasticsearch index configuration for Product read model
/// </summary>
public static class ProductIndexConfiguration
{
    public const string IndexName = "products";

    /// <summary>
    /// Creates the optimized index mapping for products with performance enhancements
    /// </summary>
    public static CreateIndexDescriptor GetIndexMapping(string indexName)
    {
        return new CreateIndexDescriptor(indexName)
            .Settings(s => s
                .NumberOfShards(5) // Optimized for better performance with larger datasets
                .NumberOfReplicas(1)
                .RefreshInterval("30s") // Optimized refresh interval for better indexing performance
                .Setting("index.max_result_window", 100000) // Increased for deep pagination
                .Setting("index.max_inner_result_window", 100000) // For nested queries
                .Setting("index.max_terms_count", 100000) // For terms aggregations
                .Setting("index.max_regex_length", 10000) // For regex queries
                .Setting("index.query.default_field", "name") // Default search field
                .Setting("index.blocks.read_only_allow_delete", false) // Prevent read-only mode
                .Setting("index.mapping.total_fields.limit", 2000) // Increase field limit
                .Setting("index.mapping.depth.limit", 20) // Increase nesting depth
                .Setting("index.mapping.nested_fields.limit", 100) // Increase nested fields
                .Setting("index.mapping.nested_objects.limit", 10000) // Increase nested objects
                .Setting("index.highlight.max_analyzed_offset", 1000000) // For highlighting
                .Setting("index.max_docvalue_fields_search", 200) // For aggregations
                .Setting("index.max_script_fields", 100) // For script fields
                .Setting("index.max_rescore_window", 10000) // For rescoring
                .Setting("index.search.slowlog.threshold.query.warn", "10s") // Slow query logging
                .Setting("index.search.slowlog.threshold.query.info", "5s")
                .Setting("index.search.slowlog.threshold.query.debug", "2s")
                .Setting("index.search.slowlog.threshold.fetch.warn", "1s")
                .Setting("index.indexing.slowlog.threshold.index.warn", "10s")
                .Setting("index.indexing.slowlog.threshold.index.info", "5s")
                .Analysis(a => a
                    .Analyzers(an => an
                        .Standard("standard_analyzer", sa => sa
                            .StopWords("_english_")
                        )
                        .Custom("product_name_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "stop", "snowball")
                        )
                        .Custom("autocomplete_analyzer", ca => ca
                            .Tokenizer("autocomplete_tokenizer")
                            .Filters("lowercase")
                        )
                        .Custom("autocomplete_search_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase")
                        )
                        .Custom("search_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "stop", "snowball", "synonym")
                        )
                    )
                    .Tokenizers(t => t
                        .EdgeNGram("autocomplete_tokenizer", en => en
                            .MinGram(2) // Increased min gram for better performance
                            .MaxGram(20)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit)
                        )
                    )
                    .TokenFilters(tf => tf
                        .Synonym("synonym", sy => sy
                            .Synonyms(
                                "smartphone,mobile,phone,cellphone",
                                "laptop,notebook,computer,pc",
                                "tv,television,monitor,display",
                                "headphones,earphones,earbuds,headset",
                                "tablet,ipad,slate",
                                "watch,smartwatch,timepiece",
                                "camera,camcorder,webcam",
                                "speaker,audio,sound",
                                "gaming,game,console,xbox,playstation",
                                "storage,disk,drive,ssd,hdd"
                            )
                        )
                        .Stop("stop_filter", st => st
                            .StopWords("_english_")
                        )
                        .Stemmer("stemmer_filter", sm => sm
                            .Language("english")
                        )
                        .Lowercase("lowercase_filter")
                        .Trim("trim_filter")
                        .WordDelimiter("word_delimiter_filter", wd => wd
                            .GenerateWordParts(true)
                            .GenerateNumberParts(true)
                            .CatenateWords(false)
                            .CatenateNumbers(false)
                            .CatenateAll(false)
                            .SplitOnCaseChange(true)
                            .PreserveOriginal(false)
                        )
                    )
                )
            )
            .Map<ProductReadModel>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t
                        .Name(n => n.Name)
                        .Analyzer("product_name_analyzer")
                        .Fields(f => f
                            .Text(tt => tt
                                .Name("autocomplete")
                                .Analyzer("autocomplete_analyzer")
                                .SearchAnalyzer("autocomplete_search_analyzer")
                            )
                            .Keyword(kk => kk.Name("keyword"))
                        )
                    )
                    .Text(t => t
                        .Name(n => n.Description)
                        .Analyzer("standard_analyzer")
                    )
                    .Keyword(k => k.Name(n => n.Sku))
                    .Number(n => n
                        .Name(nn => nn.Price)
                        .Type(NumberType.Double)
                    )
                    .Keyword(k => k.Name(n => n.Currency))
                    .Number(n => n
                        .Name(nn => nn.StockQuantity)
                        .Type(NumberType.Integer)
                    )
                    .Number(n => n
                        .Name(nn => nn.MinimumStockLevel)
                        .Type(NumberType.Integer)
                    )
                    .Object<CategoryReadModel>(o => o
                        .Name(n => n.Category)
                        .Properties(cp => cp
                            .Keyword(k => k.Name(cn => cn.Id))
                            .Text(t => t
                                .Name(cn => cn.Name)
                                .Analyzer("standard_analyzer")
                            )
                            .Text(t => t.Name(cn => cn.Description))
                            .Keyword(k => k.Name(cn => cn.ParentCategoryId))
                            .Keyword(k => k.Name(cn => cn.CategoryPath))
                        )
                    )
                    .Boolean(b => b.Name(n => n.IsActive))
                    .Boolean(b => b.Name(n => n.IsFeatured))
                    .Number(n => n
                        .Name(nn => nn.Weight)
                        .Type(NumberType.Double)
                    )
                    .Text(t => t.Name(n => n.Dimensions))
                    .Number(n => n
                        .Name(nn => nn.AverageRating)
                        .Type(NumberType.Double)
                    )
                    .Number(n => n
                        .Name(nn => nn.ReviewCount)
                        .Type(NumberType.Integer)
                    )
                    .Boolean(b => b.Name(n => n.IsInStock))
                    .Boolean(b => b.Name(n => n.IsLowStock))
                    .Boolean(b => b.Name(n => n.IsOutOfStock))
                    .Date(d => d.Name(n => n.CreatedAt))
                    .Date(d => d.Name(n => n.UpdatedAt))
                    .Keyword(k => k.Name(n => n.Tags))
                    .Completion(c => c
                        .Name(n => n.Suggest)
                        .Contexts(ctx => ctx
                            .Category(cat => cat
                                .Name("category")
                                .Path(p => p.Category.Name)
                            )
                        )
                    )
                )
            );
    }

    /// <summary>
    /// Gets search settings for product queries
    /// </summary>
    public static SearchDescriptor<ProductReadModel> GetSearchDescriptor(string indexName)
    {
        return new SearchDescriptor<ProductReadModel>()
            .Index(indexName)
            .Size(20)
            .Source(s => s.IncludeAll())
            .Highlight(h => h
                .Fields(f => f
                    .Field(p => p.Name)
                    .Field(p => p.Description)
                )
                .PreTags("<mark>")
                .PostTags("</mark>")
            );
    }

    /// <summary>
    /// Creates aggregations for product faceted search
    /// </summary>
    public static AggregationContainerDescriptor<ProductReadModel> GetAggregations()
    {
        return new AggregationContainerDescriptor<ProductReadModel>()
            .Terms("categories", t => t
                .Field(f => f.Category.Name.Suffix("keyword"))
                .Size(20)
            )
            .Range("price_ranges", r => r
                .Field(f => f.Price)
                .Ranges(
                    rng => rng.To(50),
                    rng => rng.From(50).To(100),
                    rng => rng.From(100).To(200),
                    rng => rng.From(200).To(500),
                    rng => rng.From(500)
                )
            )
            .Terms("brands", t => t
                .Field(f => f.Tags)
                .Size(10)
            )
            .Filter("in_stock", f => f
                .Filter(ff => ff.Term(t => t.IsInStock, true))
            )
            .Average("avg_rating", a => a
                .Field(f => f.AverageRating)
            );
    }
}
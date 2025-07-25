using ECommerce.ReadModel.Configuration;
using ECommerce.ReadModel.Services;
using Microsoft.Extensions.Configuration;

namespace ECommerce.ReadModel;

/// <summary>
/// Dependency injection configuration for read model services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds read model services to the service collection
    /// </summary>
    public static IServiceCollection AddReadModelServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Elasticsearch settings
        services.Configure<ElasticsearchSettings>(
            configuration.GetSection(ElasticsearchSettings.SectionName));

        // Register Elasticsearch client factory and client
        services.AddSingleton<ElasticsearchClientFactory>();
        services.AddSingleton(provider =>
        {
            var factory = provider.GetRequiredService<ElasticsearchClientFactory>();
            return factory.CreateClient();
        });

        // Register search services
        services.AddScoped<IProductSearchService, ProductSearchService>();
        services.AddScoped<IOrderSearchService, OrderSearchService>();
        services.AddScoped<ICustomerSearchService, CustomerSearchService>();

        // Register index management service
        services.AddScoped<IIndexManagementService, IndexManagementService>();

        return services;
    }

    /// <summary>
    /// Ensures all Elasticsearch indices are created
    /// </summary>
    public static async Task EnsureIndicesCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var indexManagementService = scope.ServiceProvider.GetRequiredService<IIndexManagementService>();
        
        await indexManagementService.EnsureAllIndicesCreatedAsync();
    }
}
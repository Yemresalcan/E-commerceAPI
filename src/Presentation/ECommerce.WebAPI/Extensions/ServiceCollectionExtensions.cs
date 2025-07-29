using ECommerce.Application;
using ECommerce.ReadModel;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.WebAPI.Extensions;

/// <summary>
/// Extension methods for configuring services in the DI container using modern .NET 9 patterns
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application layer services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add application layer services (MediatR, AutoMapper, FluentValidation)
        services.AddApplication();
        
        // Add read model services (Elasticsearch)
        services.AddReadModelServices(configuration);
        
        return services;
    }

    /// <summary>
    /// Adds database services with connection resilience and modern patterns
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework Core with PostgreSQL using the infrastructure extension
        services.AddDatabase(configuration);
        
        return services;
    }

    /// <summary>
    /// Adds infrastructure services (messaging, caching, search) to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add infrastructure services (RabbitMQ, Redis, etc.) using the infrastructure extension
        services.AddInfrastructure(configuration);
        
        return services;
    }

    /// <summary>
    /// Configures strongly typed configuration options using modern .NET 9 patterns
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure strongly typed options
        services.Configure<DatabaseOptions>(configuration.GetSection("ConnectionStrings"));
        services.Configure<ElasticsearchOptions>(configuration.GetSection("Elasticsearch"));
        services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
        services.Configure<CacheOptions>(configuration.GetSection("Cache"));
        services.Configure<HealthCheckOptions>(configuration.GetSection("HealthChecks"));
        
        // Validate options on startup
        services.AddOptionsWithValidateOnStart<DatabaseOptions>()
            .Bind(configuration.GetSection("ConnectionStrings"))
            .ValidateDataAnnotations();
            
        services.AddOptionsWithValidateOnStart<ElasticsearchOptions>()
            .Bind(configuration.GetSection("Elasticsearch"))
            .ValidateDataAnnotations();
            
        services.AddOptionsWithValidateOnStart<RabbitMQOptions>()
            .Bind(configuration.GetSection("RabbitMQ"))
            .ValidateDataAnnotations();
            
        services.AddOptionsWithValidateOnStart<CacheOptions>()
            .Bind(configuration.GetSection("Cache"))
            .ValidateDataAnnotations();
        
        // Add configuration validation service
        services.AddHostedService<ECommerce.WebAPI.Services.ConfigurationValidationService>();
        
        // Add startup health check service
        services.AddHostedService<ECommerce.WebAPI.Services.StartupHealthCheckService>();
        
        return services;
    }
}

/// <summary>
/// Configuration options for database connections
/// </summary>
public class DatabaseOptions
{
    public required string DefaultConnection { get; set; }
    public required string Redis { get; set; }
    public required string RabbitMQ { get; set; }
}

/// <summary>
/// Configuration options for Elasticsearch
/// </summary>
public class ElasticsearchOptions
{
    public required string Uri { get; set; }
    public required string IndexPrefix { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableDebugMode { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Configuration options for RabbitMQ
/// </summary>
public class RabbitMQOptions
{
    public required string HostName { get; set; }
    public int Port { get; set; } = 5672;
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public string VirtualHost { get; set; } = "/";
    public required string ExchangeName { get; set; }
    public required string ExchangeType { get; set; }
    public required string QueueNamePrefix { get; set; }
    public int ConnectionTimeout { get; set; } = 30;
    public int RequestTimeout { get; set; } = 30;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int NetworkRecoveryInterval { get; set; } = 10;
}

/// <summary>
/// Configuration options for caching
/// </summary>
public class CacheOptions
{
    public bool Enabled { get; set; } = true;
    public int DefaultExpirationMinutes { get; set; } = 30;
    public int ProductCacheExpirationMinutes { get; set; } = 60;
    public int OrderCacheExpirationMinutes { get; set; } = 15;
    public int CustomerCacheExpirationMinutes { get; set; } = 45;
    public int SearchCacheExpirationMinutes { get; set; } = 10;
    public required string KeyPrefix { get; set; }
}

/// <summary>
/// Configuration options for health checks
/// </summary>
public class HealthCheckOptions
{
    public int EvaluationTimeInSeconds { get; set; } = 30;
    public int MaximumHistoryEntriesPerEndpoint { get; set; } = 50;
}
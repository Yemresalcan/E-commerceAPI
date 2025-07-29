using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Caching;
using ECommerce.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ECommerce.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add messaging services (temporarily disabled for demo)
        // services.AddRabbitMQMessaging(configuration);

        // Add caching services
        services.AddRedisCaching(configuration);

        // TODO: Add other infrastructure services (persistence, etc.)

        return services;
    }

    /// <summary>
    /// Configures infrastructure services that need to be set up after the service provider is built
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task ConfigureInfrastructureAsync(this IServiceProvider serviceProvider)
    {
        // Configure event subscriptions
        await Messaging.DependencyInjection.ConfigureEventSubscriptionsAsync(serviceProvider);

        // TODO: Configure other infrastructure services
    }

    /// <summary>
    /// Adds Redis caching services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    private static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure cache options
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        // Add Redis connection
        var redisConnectionString = configuration.GetConnectionString("Redis") 
            ?? throw new InvalidOperationException("Redis connection string is not configured");

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectRetry = 3;
            configurationOptions.ConnectTimeout = 5000;
            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        // Add distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "ECommerce";
        });

        // Register cache services
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

        return services;
    }
}
using ECommerce.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

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
        // Add messaging services
        services.AddRabbitMQMessaging(configuration);

        // TODO: Add other infrastructure services (persistence, caching, etc.)

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
}
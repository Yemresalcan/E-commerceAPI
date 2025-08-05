using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Messaging.EventHandlers;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// Extension methods for registering messaging services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds RabbitMQ messaging services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddRabbitMQMessaging(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register configuration
        services.Configure<RabbitMQConfiguration>(options =>
        {
            var section = configuration.GetSection(RabbitMQConfiguration.SectionName);
            section.Bind(options);
        });

        // Register core messaging services
        services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        // Register the event bus as a hosted service
        services.AddHostedService<RabbitMQEventBus>(provider => 
            (RabbitMQEventBus)provider.GetRequiredService<IEventBus>());

        // Register event handlers
        services.AddScoped<IEventHandler<ProductCreatedEvent>, ProductCreatedEventHandler>();
        services.AddScoped<IEventHandler<ProductStockUpdatedEvent>, ProductStockUpdatedEventHandler>();
        services.AddScoped<IEventHandler<OrderPlacedEvent>, OrderPlacedEventHandler>();
        services.AddScoped<IEventHandler<CustomerRegisteredEvent>, CustomerRegisteredEventHandler>();

        return services;
    }

    /// <summary>
    /// Configures event subscriptions for the application
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task ConfigureEventSubscriptionsAsync(IServiceProvider serviceProvider)
    {
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        // Subscribe to ProductCreatedEvent
        var productCreatedHandler = serviceProvider.GetRequiredService<IEventHandler<ProductCreatedEvent>>();
        await eventBus.SubscribeAsync<ProductCreatedEvent>(async (evt, ct) => 
            await productCreatedHandler.HandleAsync(evt, ct));

        // Subscribe to ProductStockUpdatedEvent
        var productStockUpdatedHandler = serviceProvider.GetRequiredService<IEventHandler<ProductStockUpdatedEvent>>();
        await eventBus.SubscribeAsync<ProductStockUpdatedEvent>(async (evt, ct) => 
            await productStockUpdatedHandler.HandleAsync(evt, ct));

        // Subscribe to OrderPlacedEvent
        var orderPlacedHandler = serviceProvider.GetRequiredService<IEventHandler<OrderPlacedEvent>>();
        await eventBus.SubscribeAsync<OrderPlacedEvent>(async (evt, ct) => 
            await orderPlacedHandler.HandleAsync(evt, ct));

        // Subscribe to CustomerRegisteredEvent
        var customerRegisteredHandler = serviceProvider.GetRequiredService<IEventHandler<CustomerRegisteredEvent>>();
        await eventBus.SubscribeAsync<CustomerRegisteredEvent>(async (evt, ct) => 
            await customerRegisteredHandler.HandleAsync(evt, ct));
    }
}
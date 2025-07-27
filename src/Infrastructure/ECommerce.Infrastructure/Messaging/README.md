# RabbitMQ Messaging Infrastructure

This document describes how to use the RabbitMQ messaging infrastructure for publishing and handling domain events.

## Overview

The messaging infrastructure provides:
- **IEventBus**: Interface for publishing and subscribing to domain events
- **Event Handlers**: Handlers for processing specific domain events
- **Message Serialization**: JSON-based serialization for events
- **Connection Management**: Automatic connection and channel management
- **Error Handling**: Robust error handling and retry mechanisms

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "ecommerce.domain.events",
    "ExchangeType": "topic",
    "QueueNamePrefix": "ecommerce.events",
    "ConnectionTimeout": 30,
    "RequestTimeout": 30,
    "AutomaticRecoveryEnabled": true,
    "NetworkRecoveryInterval": 10
  }
}
```

## Registration

Register the messaging services in your `Program.cs`:

```csharp
// Add infrastructure services (includes messaging)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure event subscriptions after building the app
await app.Services.ConfigureInfrastructureAsync();
```

## Publishing Events

To publish a domain event, inject `IEventBus` and call `PublishAsync`:

```csharp
public class ProductService
{
    private readonly IEventBus _eventBus;

    public ProductService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateProductAsync(CreateProductCommand command)
    {
        // Create product logic...
        
        // Publish domain event
        var productCreatedEvent = new ProductCreatedEvent(
            productId: product.Id,
            name: product.Name,
            priceAmount: product.Price.Amount,
            currency: product.Price.Currency,
            categoryId: product.CategoryId,
            stockQuantity: product.StockQuantity
        );

        await _eventBus.PublishAsync(productCreatedEvent);
    }
}
```

## Creating Event Handlers

Create event handlers by implementing `IEventHandler<T>`:

```csharp
public class ProductCreatedEventHandler : IEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;
    private readonly IElasticsearchService _elasticsearchService;

    public ProductCreatedEventHandler(
        ILogger<ProductCreatedEventHandler> logger,
        IElasticsearchService elasticsearchService)
    {
        _logger = logger;
        _elasticsearchService = elasticsearchService;
    }

    public async Task HandleAsync(ProductCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ProductCreatedEvent for product {ProductId}", 
            domainEvent.ProductId);

        try
        {
            // Update read model in Elasticsearch
            await _elasticsearchService.IndexProductAsync(domainEvent, cancellationToken);
            
            // Send notifications, update analytics, etc.
            
            _logger.LogInformation("Successfully processed ProductCreatedEvent for product {ProductId}", 
                domainEvent.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ProductCreatedEvent for product {ProductId}", 
                domainEvent.ProductId);
            throw;
        }
    }
}
```

## Registering Event Handlers

Register event handlers in the DI container:

```csharp
services.AddScoped<IEventHandler<ProductCreatedEvent>, ProductCreatedEventHandler>();
```

And configure subscriptions:

```csharp
public static async Task ConfigureEventSubscriptionsAsync(IServiceProvider serviceProvider)
{
    var eventBus = serviceProvider.GetRequiredService<IEventBus>();

    // Subscribe to ProductCreatedEvent
    var productCreatedHandler = serviceProvider.GetRequiredService<IEventHandler<ProductCreatedEvent>>();
    await eventBus.SubscribeAsync<ProductCreatedEvent>(async (evt, ct) => 
        await productCreatedHandler.HandleAsync(evt, ct));
}
```

## Available Event Handlers

The following event handlers are already implemented:

- **ProductCreatedEventHandler**: Handles product creation events
- **OrderPlacedEventHandler**: Handles order placement events  
- **CustomerRegisteredEventHandler**: Handles customer registration events

## Message Flow

1. **Domain Event Creation**: Domain aggregates raise events when business operations occur
2. **Event Publishing**: Events are published to RabbitMQ exchange using topic routing
3. **Queue Binding**: Each event type has its own queue bound to the exchange
4. **Event Processing**: Event handlers consume messages and update read models
5. **Error Handling**: Failed messages are retried or sent to dead letter queues

## Routing Keys

Events are routed using the following pattern:
- **Routing Key**: `{EventName}` (lowercase, e.g., "productcreatedevent")
- **Queue Name**: `{QueuePrefix}.{EventName}` (e.g., "ecommerce.events.ProductCreatedEvent")

## Error Handling

The messaging infrastructure includes:
- **Automatic Retries**: Failed messages are automatically retried
- **Dead Letter Queues**: Messages that fail repeatedly are moved to dead letter queues
- **Logging**: All operations are logged for monitoring and debugging
- **Circuit Breaker**: Connection failures trigger automatic recovery

## Monitoring

Monitor the messaging infrastructure through:
- **Application Logs**: Structured logging with Serilog
- **RabbitMQ Management UI**: Available at http://localhost:15672
- **Health Checks**: Messaging health is included in application health checks

## Testing

The messaging infrastructure can be tested using:
- **Unit Tests**: Test event handlers in isolation
- **Integration Tests**: Test with real RabbitMQ instance
- **Test Containers**: Use Docker containers for integration testing

Example test:

```csharp
[Fact]
public async Task Should_Handle_ProductCreatedEvent()
{
    // Arrange
    var handler = new ProductCreatedEventHandler(_logger, _elasticsearchService);
    var domainEvent = new ProductCreatedEvent(/* parameters */);

    // Act
    await handler.HandleAsync(domainEvent);

    // Assert
    // Verify expected behavior
}
```
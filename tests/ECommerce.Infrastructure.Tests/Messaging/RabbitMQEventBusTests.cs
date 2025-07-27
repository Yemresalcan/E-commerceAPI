using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ECommerce.Infrastructure.Tests.Messaging;

public class RabbitMQEventBusTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<ILogger<RabbitMQEventBus>> _loggerMock;
    private readonly Mock<ILogger<RabbitMQConnectionFactory>> _connectionLoggerMock;

    public RabbitMQEventBusTests()
    {
        _loggerMock = new Mock<ILogger<RabbitMQEventBus>>();
        _connectionLoggerMock = new Mock<ILogger<RabbitMQConnectionFactory>>();

        var services = new ServiceCollection();
        
        // Configure RabbitMQ for testing
        services.Configure<RabbitMQConfiguration>(config =>
        {
            config.HostName = "localhost";
            config.Port = 5672;
            config.UserName = "guest";
            config.Password = "guest";
            config.VirtualHost = "/";
            config.ExchangeName = "test.exchange";
            config.ExchangeType = "topic";
            config.QueueNamePrefix = "test.queue";
        });

        services.AddSingleton(_loggerMock.Object);
        services.AddSingleton(_connectionLoggerMock.Object);
        services.AddSingleton<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void EventBus_Should_Be_Registered()
    {
        // Act
        var eventBus = _serviceProvider.GetService<IEventBus>();

        // Assert
        Assert.NotNull(eventBus);
        Assert.IsType<RabbitMQEventBus>(eventBus);
    }

    [Fact]
    public void MessageSerializer_Should_Serialize_And_Deserialize_Events()
    {
        // Arrange
        var serializer = _serviceProvider.GetRequiredService<IMessageSerializer>();
        var originalEvent = new ProductCreatedEvent(
            Guid.NewGuid(),
            "Test Product",
            99.99m,
            "USD",
            Guid.NewGuid(),
            10);

        // Act
        var serialized = serializer.Serialize(originalEvent);
        var deserialized = serializer.Deserialize<ProductCreatedEvent>(serialized);

        // Assert
        Assert.NotNull(serialized);
        Assert.True(serialized.Length > 0);
        Assert.NotNull(deserialized);
        Assert.Equal(originalEvent.ProductId, deserialized.ProductId);
        Assert.Equal(originalEvent.Name, deserialized.Name);
        Assert.Equal(originalEvent.PriceAmount, deserialized.PriceAmount);
        Assert.Equal(originalEvent.Currency, deserialized.Currency);
    }

    [Fact]
    public async Task EventBus_Should_Handle_Subscription_Without_Connection()
    {
        // Arrange
        var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
        var handlerCalled = false;

        // Act & Assert - Should throw exception when RabbitMQ is not available
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await eventBus.SubscribeAsync<ProductCreatedEvent>(async (evt, ct) =>
            {
                handlerCalled = true;
                await Task.CompletedTask;
            });
        });

        Assert.Contains("Channel is not initialized", exception.Message);
        Assert.False(handlerCalled); // Handler shouldn't be called during subscription
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
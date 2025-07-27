using System.Collections.Concurrent;
using System.Text;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of the event bus
/// </summary>
public class RabbitMQEventBus : IEventBus, IHostedService, IDisposable
{
    private readonly IRabbitMQConnectionFactory _connectionFactory;
    private readonly IMessageSerializer _messageSerializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMQConfiguration _configuration;
    private readonly ILogger<RabbitMQEventBus> _logger;
    
    private readonly ConcurrentDictionary<string, List<Func<object, CancellationToken, Task>>> _handlers = new();
    private readonly ConcurrentDictionary<string, Type> _eventTypes = new();
    
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public RabbitMQEventBus(
        IRabbitMQConnectionFactory connectionFactory,
        IMessageSerializer messageSerializer,
        IServiceProvider serviceProvider,
        IOptions<RabbitMQConfiguration> configuration,
        ILogger<RabbitMQEventBus> logger)
    {
        _connectionFactory = connectionFactory;
        _messageSerializer = messageSerializer;
        _serviceProvider = serviceProvider;
        _configuration = configuration.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : class
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQEventBus));

        try
        {
            EnsureConnection();
            
            var eventName = GetEventName<T>();
            var routingKey = GetRoutingKey<T>();
            var message = _messageSerializer.Serialize(domainEvent);
            
            var properties = _channel!.CreateBasicProperties();
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = eventName;
            properties.ContentType = "application/json";
            properties.ContentEncoding = "utf-8";
            properties.DeliveryMode = 2; // Persistent

            _channel!.BasicPublish(
                exchange: _configuration.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: message);

            _logger.LogDebug("Published event {EventName} with routing key {RoutingKey}", 
                eventName, routingKey);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler) 
        where T : class
    {
        ArgumentNullException.ThrowIfNull(handler);
        
        var eventName = GetEventName<T>();
        
        _eventTypes.TryAdd(eventName, typeof(T));
        
        var handlers = _handlers.GetOrAdd(eventName, _ => new List<Func<object, CancellationToken, Task>>());
        
        handlers.Add(async (evt, ct) =>
        {
            if (evt is T typedEvent)
            {
                await handler(typedEvent, ct);
            }
        });

        EnsureSubscription<T>();
        
        _logger.LogInformation("Subscribed to event {EventName}", eventName);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting RabbitMQ Event Bus");
        
        EnsureConnection();
        SetupExchange();
        
        _logger.LogInformation("RabbitMQ Event Bus started successfully");
        
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping RabbitMQ Event Bus");
        
        try
        {
            if (_channel?.IsOpen == true)
            {
                _channel.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing RabbitMQ channel");
        }
        
        _logger.LogInformation("RabbitMQ Event Bus stopped");
        
        return Task.CompletedTask;
    }

    private void EnsureConnection()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        _connection = _connectionFactory.CreateConnection();
        _channel = _connectionFactory.CreateChannel(_connection);
        
        SetupExchange();
    }

    private void SetupExchange()
    {
        if (_channel == null)
            throw new InvalidOperationException("Channel is not initialized");

        _channel.ExchangeDeclare(
            exchange: _configuration.ExchangeName,
            type: _configuration.ExchangeType,
            durable: true,
            autoDelete: false);

        _logger.LogDebug("Exchange {ExchangeName} declared", _configuration.ExchangeName);
    }

    private void EnsureSubscription<T>() where T : class
    {
        if (_channel == null)
            throw new InvalidOperationException("Channel is not initialized");

        var eventName = GetEventName<T>();
        var queueName = GetQueueName<T>();
        var routingKey = GetRoutingKey<T>();

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: queueName,
            exchange: _configuration.ExchangeName,
            routingKey: routingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, eventArgs) =>
        {
            await ProcessMessageAsync(eventArgs);
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogDebug("Consumer setup for queue {QueueName} with routing key {RoutingKey}", 
            queueName, routingKey);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.BasicProperties.Type;
        var message = eventArgs.Body.ToArray();

        try
        {
            if (_eventTypes.TryGetValue(eventName, out var eventType) &&
                _handlers.TryGetValue(eventName, out var eventHandlers))
            {
                var domainEvent = _messageSerializer.Deserialize(message, eventType);
                
                var tasks = eventHandlers.Select(handler => 
                    handler(domainEvent, CancellationToken.None));
                
                await Task.WhenAll(tasks);
                
                _channel!.BasicAck(eventArgs.DeliveryTag, false);
                
                _logger.LogDebug("Processed event {EventName} successfully", eventName);
            }
            else
            {
                _logger.LogWarning("No handlers found for event {EventName}", eventName);
                _channel!.BasicNack(eventArgs.DeliveryTag, false, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventName}", eventName);
            _channel!.BasicNack(eventArgs.DeliveryTag, false, true);
        }
    }

    private static string GetEventName<T>() => typeof(T).Name;
    
    private string GetQueueName<T>() => $"{_configuration.QueueNamePrefix}.{GetEventName<T>()}";
    
    private static string GetRoutingKey<T>() => typeof(T).Name.ToLowerInvariant();

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ Event Bus");
        }
        finally
        {
            _disposed = true;
        }
    }
}
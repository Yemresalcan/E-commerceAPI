using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// Factory for creating RabbitMQ connections and channels
/// </summary>
public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory, IDisposable
{
    private readonly RabbitMQConfiguration _configuration;
    private readonly ILogger<RabbitMQConnectionFactory> _logger;
    private readonly ConnectionFactory _connectionFactory;
    private IConnection? _connection;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMQConnectionFactory(
        IOptions<RabbitMQConfiguration> configuration,
        ILogger<RabbitMQConnectionFactory> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
        
        _connectionFactory = new ConnectionFactory
        {
            HostName = _configuration.HostName,
            Port = _configuration.Port,
            UserName = _configuration.UserName,
            Password = _configuration.Password,
            VirtualHost = _configuration.VirtualHost,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(_configuration.ConnectionTimeout),
            RequestedHeartbeat = TimeSpan.FromSeconds(_configuration.RequestTimeout),
            AutomaticRecoveryEnabled = _configuration.AutomaticRecoveryEnabled,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(_configuration.NetworkRecoveryInterval),
            DispatchConsumersAsync = true
        };
    }

    /// <inheritdoc />
    public IConnection CreateConnection()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQConnectionFactory));

        lock (_lock)
        {
            if (_connection?.IsOpen == true)
                return _connection;

            try
            {
                _logger.LogInformation("Creating RabbitMQ connection to {HostName}:{Port}", 
                    _configuration.HostName, _configuration.Port);
                
                _connection?.Dispose();
                _connection = _connectionFactory.CreateConnection();
                
                _connection.ConnectionShutdown += (sender, args) =>
                {
                    _logger.LogWarning("RabbitMQ connection shutdown: {Reason}", args.ReplyText);
                };

                _connection.CallbackException += (sender, args) =>
                {
                    _logger.LogError(args.Exception, "RabbitMQ connection callback exception");
                };

                _logger.LogInformation("RabbitMQ connection established successfully");
                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RabbitMQ connection");
                throw;
            }
        }
    }

    /// <inheritdoc />
    public IModel CreateChannel(IConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQConnectionFactory));

        try
        {
            var channel = connection.CreateModel();
            
            _logger.LogDebug("Created new RabbitMQ channel");
            return channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ channel");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            if (_disposed)
                return;

            try
            {
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
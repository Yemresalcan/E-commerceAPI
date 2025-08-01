using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace ECommerce.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Database interceptor for performance monitoring and optimization
/// </summary>
public class PerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<PerformanceInterceptor> _logger;
    private const int SlowQueryThresholdMs = 1000; // 1 second

    public PerformanceInterceptor()
    {
        // Use a factory pattern or service locator in production
        var loggerFactory = LoggerFactory.Create(builder => { });
        _logger = loggerFactory.CreateLogger<PerformanceInterceptor>();
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        var duration = eventData.Duration.TotalMilliseconds;
        
        if (duration > SlowQueryThresholdMs)
        {
            _logger.LogWarning(
                "Slow query detected: {Duration}ms - {CommandText}",
                duration,
                command.CommandText);
        }
        else
        {
            _logger.LogDebug(
                "Query executed in {Duration}ms",
                duration);
        }

        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var duration = eventData.Duration.TotalMilliseconds;
        
        if (duration > SlowQueryThresholdMs)
        {
            _logger.LogWarning(
                "Slow non-query command detected: {Duration}ms - {CommandText}",
                duration,
                command.CommandText);
        }

        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        var duration = eventData.Duration.TotalMilliseconds;
        
        if (duration > SlowQueryThresholdMs)
        {
            _logger.LogWarning(
                "Slow scalar command detected: {Duration}ms - {CommandText}",
                duration,
                command.CommandText);
        }

        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(
            eventData.Exception,
            "Database command failed: {CommandText}",
            command.CommandText);

        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }
}
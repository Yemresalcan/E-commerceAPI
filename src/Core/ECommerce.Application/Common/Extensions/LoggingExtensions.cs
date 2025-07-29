namespace ECommerce.Application.Common.Extensions;

/// <summary>
/// Extension methods for structured logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Creates a logging scope for command operations
    /// </summary>
    public static IDisposable? BeginCommandScope<T>(this ILogger logger, string commandName, T command)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "Command",
            ["CommandName"] = commandName,
            ["CommandType"] = typeof(T).FullName!,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Creates a logging scope for query operations
    /// </summary>
    public static IDisposable? BeginQueryScope<T>(this ILogger logger, string queryName, T query)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "Query",
            ["QueryName"] = queryName,
            ["QueryType"] = typeof(T).FullName!,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Creates a logging scope for domain operations
    /// </summary>
    public static IDisposable? BeginDomainScope(this ILogger logger, string aggregateName, Guid aggregateId, string operation)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "Domain",
            ["AggregateName"] = aggregateName,
            ["AggregateId"] = aggregateId,
            ["DomainOperation"] = operation,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Creates a logging scope for repository operations
    /// </summary>
    public static IDisposable? BeginRepositoryScope(this ILogger logger, string repositoryName, string operation)
    {
        return logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "Repository",
            ["RepositoryName"] = repositoryName,
            ["RepositoryOperation"] = operation,
            ["Timestamp"] = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Logs performance metrics for critical operations
    /// </summary>
    public static void LogPerformanceMetrics(this ILogger logger, string operationName, TimeSpan duration, long memoryUsed = 0, Dictionary<string, object>? additionalMetrics = null)
    {
        var metrics = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["DurationMs"] = duration.TotalMilliseconds,
            ["MemoryUsedBytes"] = memoryUsed,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (additionalMetrics != null)
        {
            foreach (var metric in additionalMetrics)
            {
                metrics[metric.Key] = metric.Value;
            }
        }

        using (logger.BeginScope(metrics))
        {
            if (duration.TotalMilliseconds > 2000)
            {
                logger.LogError("Critical performance issue in operation {OperationName}: {DurationMs}ms", operationName, duration.TotalMilliseconds);
            }
            else if (duration.TotalMilliseconds > 500)
            {
                logger.LogWarning("Slow operation detected {OperationName}: {DurationMs}ms", operationName, duration.TotalMilliseconds);
            }
            else
            {
                logger.LogDebug("Operation {OperationName} completed in {DurationMs}ms", operationName, duration.TotalMilliseconds);
            }
        }
    }

    /// <summary>
    /// Logs business rule validation results
    /// </summary>
    public static void LogBusinessRuleValidation(this ILogger logger, string ruleName, bool isValid, string? reason = null)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["BusinessRule"] = ruleName,
            ["IsValid"] = isValid,
            ["ValidationReason"] = reason ?? "N/A",
            ["Timestamp"] = DateTimeOffset.UtcNow
        }))
        {
            if (isValid)
            {
                logger.LogDebug("Business rule {BusinessRule} validation passed", ruleName);
            }
            else
            {
                logger.LogWarning("Business rule {BusinessRule} validation failed: {ValidationReason}", ruleName, reason);
            }
        }
    }

    /// <summary>
    /// Logs domain event information
    /// </summary>
    public static void LogDomainEvent(this ILogger logger, string eventName, Guid aggregateId, Dictionary<string, object>? eventData = null)
    {
        var logData = new Dictionary<string, object>
        {
            ["EventName"] = eventName,
            ["AggregateId"] = aggregateId,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (eventData != null)
        {
            foreach (var data in eventData)
            {
                logData[$"Event_{data.Key}"] = data.Value;
            }
        }

        using (logger.BeginScope(logData))
        {
            logger.LogInformation("Domain event {EventName} raised for aggregate {AggregateId}", eventName, aggregateId);
        }
    }
}
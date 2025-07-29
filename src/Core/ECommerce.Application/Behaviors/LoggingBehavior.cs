using System.Diagnostics;
using System.Text.Json;

namespace ECommerce.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs request and response information with structured logging
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["RequestName"] = requestName,
            ["RequestType"] = typeof(TRequest).FullName!
        }))
        {
            logger.LogInformation("Starting request {RequestName} with ID {RequestId}", requestName, requestId);

            // Log request details for commands (but not queries to avoid logging sensitive data)
            if (requestName.EndsWith("Command"))
            {
                try
                {
                    var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                    { 
                        WriteIndented = false,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    logger.LogDebug("Request details for {RequestName}: {RequestData}", requestName, requestJson);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to serialize request {RequestName} for logging", requestName);
                }
            }

            try
            {
                var response = await next();
                
                stopwatch.Stop();
                logger.LogInformation("Successfully completed request {RequestName} with ID {RequestId} in {ElapsedMilliseconds}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
                
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed after {ElapsedMilliseconds}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
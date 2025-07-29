using System.Diagnostics;

namespace ECommerce.Application.Behaviors;

/// <summary>
/// Pipeline behavior that monitors performance and logs slow requests with detailed metrics
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;
    private const int CriticalRequestThresholdMs = 2000;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        var startMemory = GC.GetTotalMemory(false);
        
        var response = await next();
        
        stopwatch.Stop();
        var endMemory = GC.GetTotalMemory(false);
        var memoryUsed = endMemory - startMemory;
        
        var elapsedMs = stopwatch.ElapsedMilliseconds;
        
        // Log performance metrics with structured data
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestName"] = requestName,
            ["ElapsedMilliseconds"] = elapsedMs,
            ["MemoryUsedBytes"] = memoryUsed,
            ["IsSlowRequest"] = elapsedMs > SlowRequestThresholdMs,
            ["IsCriticalRequest"] = elapsedMs > CriticalRequestThresholdMs
        }))
        {
            if (elapsedMs > CriticalRequestThresholdMs)
            {
                logger.LogError("Critical performance issue: {RequestName} took {ElapsedMilliseconds}ms and used {MemoryUsedBytes} bytes", 
                    requestName, elapsedMs, memoryUsed);
            }
            else if (elapsedMs > SlowRequestThresholdMs)
            {
                logger.LogWarning("Slow request detected: {RequestName} took {ElapsedMilliseconds}ms and used {MemoryUsedBytes} bytes", 
                    requestName, elapsedMs, memoryUsed);
            }
            else
            {
                logger.LogDebug("Request {RequestName} completed in {ElapsedMilliseconds}ms using {MemoryUsedBytes} bytes", 
                    requestName, elapsedMs, memoryUsed);
            }
        }
        
        return response;
    }
}
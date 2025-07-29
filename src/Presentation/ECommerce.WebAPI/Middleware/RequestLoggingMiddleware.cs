using System.Diagnostics;
using System.Text;

namespace ECommerce.WebAPI.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with structured logging
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private const int MaxBodySize = 4096; // 4KB limit for request/response body logging

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        // Add request ID to response headers for tracing
        context.Response.Headers.Append("X-Request-ID", requestId.ToString());

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
            ["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        }))
        {
            // Log request
            await LogRequestAsync(context, requestId);

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Unhandled exception occurred during request {RequestId} processing after {ElapsedMs}ms", 
                    requestId, stopwatch.ElapsedMilliseconds);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                // Log response
                await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);

                // Copy response back to original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }

    private async Task LogRequestAsync(HttpContext context, Guid requestId)
    {
        var request = context.Request;
        
        var logData = new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Method"] = request.Method,
            ["Path"] = request.Path,
            ["QueryString"] = request.QueryString.ToString(),
            ["ContentType"] = request.ContentType ?? "none",
            ["ContentLength"] = request.ContentLength ?? 0,
            ["Scheme"] = request.Scheme,
            ["Host"] = request.Host.ToString()
        };

        // Add headers (excluding sensitive ones)
        var headers = request.Headers
            .Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(h => $"Header_{h.Key}", h => string.Join(", ", h.Value.ToArray()));
        
        foreach (var header in headers)
        {
            logData[header.Key] = header.Value;
        }

        using (logger.BeginScope(logData))
        {
            logger.LogInformation("HTTP {Method} {Path} started", request.Method, request.Path);

            // Log request body for POST/PUT requests (if not too large)
            if (ShouldLogRequestBody(request))
            {
                var body = await ReadRequestBodyAsync(request);
                if (!string.IsNullOrEmpty(body))
                {
                    logger.LogDebug("Request body: {RequestBody}", body);
                }
            }
        }
    }

    private async Task LogResponseAsync(HttpContext context, Guid requestId, long elapsedMs)
    {
        var response = context.Response;
        
        var logData = new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["StatusCode"] = response.StatusCode,
            ["ContentType"] = response.ContentType ?? "none",
            ["ContentLength"] = response.ContentLength ?? response.Body.Length,
            ["ElapsedMs"] = elapsedMs
        };

        using (logger.BeginScope(logData))
        {
            var logLevel = GetLogLevelForStatusCode(response.StatusCode);
            
            logger.Log(logLevel, "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms", 
                context.Request.Method, context.Request.Path, response.StatusCode, elapsedMs);

            // Log response body for errors (if not too large)
            if (response.StatusCode >= 400 && ShouldLogResponseBody(response))
            {
                var body = await ReadResponseBodyAsync(response);
                if (!string.IsNullOrEmpty(body))
                {
                    logger.LogDebug("Response body: {ResponseBody}", body);
                }
            }

            // Log performance warning for slow requests
            if (elapsedMs > 1000)
            {
                logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMs}ms", 
                    context.Request.Method, context.Request.Path, elapsedMs);
            }
        }
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        if (request.ContentLength == null || request.ContentLength == 0)
            return false;

        if (request.ContentLength > MaxBodySize)
            return false;

        var contentType = request.ContentType?.ToLower();
        return contentType != null && (
            contentType.Contains("application/json") ||
            contentType.Contains("application/xml") ||
            contentType.Contains("text/"));
    }

    private static bool ShouldLogResponseBody(HttpResponse response)
    {
        var contentLength = response.ContentLength ?? response.Body.Length;
        if (contentLength > MaxBodySize)
            return false;

        var contentType = response.ContentType?.ToLower();
        return contentType != null && (
            contentType.Contains("application/json") ||
            contentType.Contains("application/xml") ||
            contentType.Contains("text/"));
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body.Length > MaxBodySize ? body[..MaxBodySize] + "... [truncated]" : body;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return body.Length > MaxBodySize ? body[..MaxBodySize] + "... [truncated]" : body;
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "authorization", "cookie", "x-api-key", "x-auth-token", 
            "authentication", "proxy-authorization"
        };

        return sensitiveHeaders.Contains(headerName.ToLower());
    }

    private static LogLevel GetLogLevelForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
}
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Exceptions;
using ECommerce.WebAPI.Models;
using FluentValidation;

namespace ECommerce.WebAPI.Middleware;

/// <summary>
/// Middleware for handling exceptions globally and returning appropriate HTTP responses
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        object errorResponse = exception switch
        {
            FluentValidation.ValidationException validationEx => CreateValidationErrorResponse(context, validationEx),
            ECommerce.Application.Exceptions.ValidationException appValidationEx => CreateApplicationValidationErrorResponse(context, appValidationEx),
            NotFoundException notFoundEx => CreateErrorResponse(context, notFoundEx.Message, HttpStatusCode.NotFound, "Resource Not Found"),
            ConflictException conflictEx => CreateErrorResponse(context, conflictEx.Message, HttpStatusCode.Conflict, "Conflict"),
            DomainException domainEx => CreateErrorResponse(context, domainEx.Message, HttpStatusCode.BadRequest, "Domain Rule Violation"),
            UnauthorizedAccessException => CreateErrorResponse(context, "Access denied", HttpStatusCode.Unauthorized, "Unauthorized"),
            ArgumentException argEx => CreateErrorResponse(context, argEx.Message, HttpStatusCode.BadRequest, "Invalid Argument"),
            _ => CreateErrorResponse(context, "An internal server error occurred", HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        response.StatusCode = errorResponse switch
        {
            ErrorResponse er => er.Status,
            ValidationErrorResponse ver => ver.Status,
            _ => 500
        };
        var jsonResponse = JsonSerializer.Serialize(errorResponse, JsonOptions);
        await response.WriteAsync(jsonResponse);
    }

    private static ErrorResponse CreateErrorResponse(HttpContext context, string detail, HttpStatusCode statusCode, string title)
    {
        return new ErrorResponse
        {
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path,
            Type = GetProblemTypeUri(statusCode),
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };
    }

    private static ValidationErrorResponse CreateValidationErrorResponse(HttpContext context, FluentValidation.ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

        var response = new ValidationErrorResponse
        {
            Title = "Validation Failed",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "One or more validation errors occurred",
            Instance = context.Request.Path,
            Type = GetProblemTypeUri(HttpStatusCode.BadRequest),
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = errors
        };

        return response;
    }

    private static ValidationErrorResponse CreateApplicationValidationErrorResponse(HttpContext context, ECommerce.Application.Exceptions.ValidationException validationException)
    {
        return new ValidationErrorResponse
        {
            Title = "Validation Failed",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = validationException.Message,
            Instance = context.Request.Path,
            Type = GetProblemTypeUri(HttpStatusCode.BadRequest),
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Errors = validationException.Errors
        };
    }

    private static string GetProblemTypeUri(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            HttpStatusCode.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            HttpStatusCode.UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
            HttpStatusCode.InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => "about:blank"
        };
    }
}
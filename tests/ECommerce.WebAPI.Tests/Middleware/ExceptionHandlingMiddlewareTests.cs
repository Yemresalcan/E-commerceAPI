using System.Net;
using System.Text.Json;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Exceptions;
using ECommerce.WebAPI.Middleware;
using ECommerce.WebAPI.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.WebAPI.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
    private readonly ExceptionHandlingMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        
        _middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Test exception"),
            _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithFluentValidationException_ReturnsValidationErrorResponse()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email is invalid")
        };
        var validationException = new FluentValidation.ValidationException(validationFailures);
        
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw validationException,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        Assert.Equal("application/json", _httpContext.Response.ContentType);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        
        // Debug: Check what we actually got
        Assert.False(string.IsNullOrEmpty(responseBody), "Response body should not be empty");
        
        // First try to deserialize as JsonElement to see the structure
        var jsonDoc = JsonDocument.Parse(responseBody);
        var hasErrors = jsonDoc.RootElement.TryGetProperty("errors", out var errorsProperty);
        
        Assert.True(hasErrors, $"Response should have 'errors' property. Response: {responseBody}");
        
        var errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Validation Failed", errorResponse.Title);
        Assert.NotNull(errorResponse.Errors);
        Assert.Equal(2, errorResponse.Errors.Count);
        Assert.Contains("Name", errorResponse.Errors.Keys);
        Assert.Contains("Email", errorResponse.Errors.Keys);
    }

    [Fact]
    public async Task InvokeAsync_WithApplicationValidationException_ReturnsValidationErrorResponse()
    {
        // Arrange
        var validationException = new ECommerce.Application.Exceptions.ValidationException("Name", "Name is required");
        
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw validationException,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        
        // Debug: Check what we actually got
        Assert.False(string.IsNullOrEmpty(responseBody), "Response body should not be empty");
        
        var errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Validation Failed", errorResponse.Title);
        Assert.NotNull(errorResponse.Errors);
        Assert.True(errorResponse.Errors.Count >= 1, $"Expected at least 1 error, got {errorResponse.Errors.Count}. Response: {responseBody}");
        Assert.Contains("Name", errorResponse.Errors.Keys);
    }

    [Fact]
    public async Task InvokeAsync_WithNotFoundException_ReturnsNotFoundResponse()
    {
        // Arrange
        var notFoundException = new NotFoundException("Product", Guid.NewGuid());
        
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw notFoundException,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Resource Not Found", errorResponse.Title);
        Assert.Equal((int)HttpStatusCode.NotFound, errorResponse.Status);
    }

    [Fact]
    public async Task InvokeAsync_WithDomainException_ReturnsBadRequestResponse()
    {
        // Arrange
        var domainException = new InvalidStockQuantityException(-5);
        
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw domainException,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Domain Rule Violation", errorResponse.Title);
        Assert.Equal((int)HttpStatusCode.BadRequest, errorResponse.Status);
        Assert.Contains("Stock quantity cannot be negative", errorResponse.Detail);
    }

    [Fact]
    public async Task InvokeAsync_WithConflictException_ReturnsConflictResponse()
    {
        // Arrange
        var conflictException = new ConflictException("Email already exists");
        
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw conflictException,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.Conflict, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Conflict", errorResponse.Title);
        Assert.Equal((int)HttpStatusCode.Conflict, errorResponse.Status);
        Assert.Equal("Email already exists", errorResponse.Detail);
    }

    [Fact]
    public async Task InvokeAsync_WithUnhandledException_ReturnsInternalServerErrorResponse()
    {
        // Arrange
        var unhandledException = new NullReferenceException("Unexpected error");
        
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw unhandledException,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Internal Server Error", errorResponse.Title);
        Assert.Equal((int)HttpStatusCode.InternalServerError, errorResponse.Status);
        Assert.Equal("An internal server error occurred", errorResponse.Detail);
    }

    [Fact]
    public async Task InvokeAsync_LogsException()
    {
        // Arrange
        var exception = new NullReferenceException("Test exception");
        
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unhandled exception occurred while processing the request")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
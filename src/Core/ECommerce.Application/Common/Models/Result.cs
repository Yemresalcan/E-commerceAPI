namespace ECommerce.Application.Common.Models;

/// <summary>
/// Represents the result of an operation
/// </summary>
public class Result
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new() { IsSuccess = true };

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static Result Failure(string error) => new() { IsSuccess = false, Error = error };
}

/// <summary>
/// Represents the result of an operation that returns a value
/// </summary>
/// <typeparam name="T">The type of value returned</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// The value returned by the operation
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static new Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
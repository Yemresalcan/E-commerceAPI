namespace ECommerce.Application.Exceptions;

/// <summary>
/// Exception thrown when a request conflicts with the current state of the resource
/// </summary>
public class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
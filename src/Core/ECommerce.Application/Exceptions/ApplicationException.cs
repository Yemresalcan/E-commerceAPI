namespace ECommerce.Application.Exceptions;

/// <summary>
/// Base class for all application-specific exceptions
/// </summary>
public abstract class ApplicationException : Exception
{
    protected ApplicationException(string message) : base(message)
    {
    }

    protected ApplicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
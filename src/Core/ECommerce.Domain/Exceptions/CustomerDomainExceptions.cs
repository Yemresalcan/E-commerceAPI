namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when a customer domain rule is violated
/// </summary>
public class CustomerDomainException : DomainException
{
    public CustomerDomainException(string message) : base(message)
    {
    }

    public CustomerDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when attempting to register a customer with an email that already exists
/// </summary>
public class DuplicateEmailException : CustomerDomainException
{
    public string Email { get; }

    public DuplicateEmailException(string email)
        : base($"A customer with email '{email}' already exists")
    {
        Email = email;
    }
}

/// <summary>
/// Exception thrown when attempting to create a customer with invalid email format
/// </summary>
public class InvalidEmailFormatException : CustomerDomainException
{
    public string Email { get; }

    public InvalidEmailFormatException(string email)
        : base($"Invalid email format: '{email}'")
    {
        Email = email;
    }
}

/// <summary>
/// Exception thrown when attempting to create a customer with invalid phone number format
/// </summary>
public class InvalidPhoneNumberFormatException : CustomerDomainException
{
    public string PhoneNumber { get; }

    public InvalidPhoneNumberFormatException(string phoneNumber)
        : base($"Invalid phone number format: '{phoneNumber}'")
    {
        PhoneNumber = phoneNumber;
    }
}

/// <summary>
/// Exception thrown when attempting to add an invalid address
/// </summary>
public class InvalidAddressException : CustomerDomainException
{
    public InvalidAddressException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when attempting to access a customer that doesn't exist
/// </summary>
public class CustomerNotFoundException : CustomerDomainException
{
    public Guid CustomerId { get; }

    public CustomerNotFoundException(Guid customerId)
        : base($"Customer with ID '{customerId}' was not found")
    {
        CustomerId = customerId;
    }
}
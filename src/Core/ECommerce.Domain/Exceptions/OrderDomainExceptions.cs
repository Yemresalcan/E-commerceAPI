using ECommerce.Domain.Aggregates.OrderAggregate;

namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when an order domain rule is violated
/// </summary>
public class OrderDomainException : DomainException
{
    public OrderDomainException(string message) : base(message)
    {
    }

    public OrderDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an invalid operation is attempted on an order in a specific state
/// </summary>
public class InvalidOrderStateException : OrderDomainException
{
    public OrderStatus CurrentState { get; }
    public string AttemptedAction { get; }

    public InvalidOrderStateException(OrderStatus currentState, string attemptedAction)
        : base($"Cannot {attemptedAction} order in {currentState} state")
    {
        CurrentState = currentState;
        AttemptedAction = attemptedAction;
    }
}

/// <summary>
/// Exception thrown when an invalid operation is attempted on a payment in a specific state
/// </summary>
public class InvalidPaymentStateException : OrderDomainException
{
    public PaymentStatus CurrentState { get; }
    public string AttemptedAction { get; }

    public InvalidPaymentStateException(PaymentStatus currentState, string attemptedAction)
        : base($"Cannot {attemptedAction} payment in {currentState} state")
    {
        CurrentState = currentState;
        AttemptedAction = attemptedAction;
    }
}
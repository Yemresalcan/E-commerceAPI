namespace ECommerce.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Represents the various states a payment can be in during its lifecycle
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment has been created but not yet processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is currently being processed by the payment provider
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment has been successfully completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment has failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment has been refunded
    /// </summary>
    Refunded = 4
}
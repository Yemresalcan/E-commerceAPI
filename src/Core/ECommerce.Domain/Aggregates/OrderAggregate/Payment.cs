using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Represents payment information and status tracking for an order
/// </summary>
public class Payment : Entity
{
    /// <summary>
    /// The payment amount
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// The currency of the payment
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// The payment method used (e.g., CreditCard, PayPal, BankTransfer)
    /// </summary>
    public PaymentMethod Method { get; private set; }

    /// <summary>
    /// Current status of the payment
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// External payment provider transaction ID
    /// </summary>
    public string? TransactionId { get; private set; }

    /// <summary>
    /// Payment provider reference (e.g., Stripe, PayPal)
    /// </summary>
    public string? PaymentProvider { get; private set; }

    /// <summary>
    /// Date and time when payment was processed
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Failure reason if payment failed
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// The order this payment belongs to
    /// </summary>
    public Guid OrderId { get; private set; }

    // Private constructor for EF Core
    private Payment() { }

    /// <summary>
    /// Creates a new payment
    /// </summary>
    public static Payment Create(
        decimal amount,
        string currency,
        PaymentMethod method,
        string? paymentProvider = null)
    {
        if (amount <= 0)
            throw new OrderDomainException("Payment amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(currency))
            throw new OrderDomainException("Payment currency is required");

        if (currency.Length != 3)
            throw new OrderDomainException("Currency must be a 3-character ISO code");

        return new Payment
        {
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Method = method,
            Status = PaymentStatus.Pending,
            PaymentProvider = paymentProvider
        };
    }

    /// <summary>
    /// Marks the payment as processing
    /// </summary>
    public void MarkAsProcessing(string? transactionId = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidPaymentStateException(Status, "mark as processing");

        Status = PaymentStatus.Processing;
        TransactionId = transactionId;
        MarkAsModified();
    }

    /// <summary>
    /// Marks the payment as completed successfully
    /// </summary>
    public void MarkAsCompleted(string transactionId)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidPaymentStateException(Status, "complete");

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new OrderDomainException("Transaction ID is required for completed payment");

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
        FailureReason = null; // Clear any previous failure reason
        MarkAsModified();
    }

    /// <summary>
    /// Marks the payment as failed
    /// </summary>
    public void MarkAsFailed(string failureReason)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidPaymentStateException(Status, "mark as failed");

        if (string.IsNullOrWhiteSpace(failureReason))
            throw new OrderDomainException("Failure reason is required for failed payment");

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        ProcessedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// Marks the payment as refunded
    /// </summary>
    public void MarkAsRefunded(string refundTransactionId)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidPaymentStateException(Status, "refund");

        if (string.IsNullOrWhiteSpace(refundTransactionId))
            throw new OrderDomainException("Refund transaction ID is required");

        Status = PaymentStatus.Refunded;
        TransactionId = refundTransactionId; // Update with refund transaction ID
        MarkAsModified();
    }

    /// <summary>
    /// Retries a failed payment
    /// </summary>
    public void Retry()
    {
        if (Status != PaymentStatus.Failed)
            throw new InvalidPaymentStateException(Status, "retry");

        Status = PaymentStatus.Pending;
        FailureReason = null;
        TransactionId = null;
        ProcessedAt = null;
        MarkAsModified();
    }

    /// <summary>
    /// Checks if the payment is in a final state (completed, failed, or refunded)
    /// </summary>
    public bool IsInFinalState => Status is PaymentStatus.Completed or PaymentStatus.Failed or PaymentStatus.Refunded;

    /// <summary>
    /// Checks if the payment is successful
    /// </summary>
    public bool IsSuccessful => Status == PaymentStatus.Completed;

    /// <summary>
    /// Sets the order ID (used internally by the Order aggregate)
    /// </summary>
    internal void SetOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}
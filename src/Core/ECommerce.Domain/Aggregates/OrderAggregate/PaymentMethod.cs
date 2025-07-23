namespace ECommerce.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Represents the various payment methods supported by the system
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Credit card payment
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// Debit card payment
    /// </summary>
    DebitCard = 1,

    /// <summary>
    /// PayPal payment
    /// </summary>
    PayPal = 2,

    /// <summary>
    /// Bank transfer payment
    /// </summary>
    BankTransfer = 3,

    /// <summary>
    /// Digital wallet payment (e.g., Apple Pay, Google Pay)
    /// </summary>
    DigitalWallet = 4,

    /// <summary>
    /// Cash on delivery
    /// </summary>
    CashOnDelivery = 5
}
using System.Globalization;

namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Represents a monetary value with currency support and arithmetic operations.
/// </summary>
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-character ISO code.", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a zero money value with the specified currency.
    /// </summary>
    public static Money Zero(string currency) => new(0, currency);

    /// <summary>
    /// Adds two money values. Both must have the same currency.
    /// </summary>
    public Money Add(Money other)
    {
        ValidateSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts one money value from another. Both must have the same currency.
    /// </summary>
    public Money Subtract(Money other)
    {
        ValidateSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the money value by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Divides the money value by a divisor.
    /// </summary>
    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide money by zero.");

        return new Money(Amount / divisor, Currency);
    }

    /// <summary>
    /// Checks if this money value is greater than another.
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount > other.Amount;
    }

    /// <summary>
    /// Checks if this money value is less than another.
    /// </summary>
    public bool IsLessThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount < other.Amount;
    }

    /// <summary>
    /// Checks if this money value is greater than or equal to another.
    /// </summary>
    public bool IsGreaterThanOrEqual(Money other)
    {
        ValidateSameCurrency(other);
        return Amount >= other.Amount;
    }

    /// <summary>
    /// Checks if this money value is less than or equal to another.
    /// </summary>
    public bool IsLessThanOrEqual(Money other)
    {
        ValidateSameCurrency(other);
        return Amount <= other.Amount;
    }

    /// <summary>
    /// Checks if this money value is positive.
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Checks if this money value is negative.
    /// </summary>
    public bool IsNegative => Amount < 0;

    /// <summary>
    /// Checks if this money value is zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    private void ValidateSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot perform operation on different currencies: {Currency} and {other.Currency}");
    }

    public override string ToString()
    {
        return $"{Amount.ToString("F2", CultureInfo.InvariantCulture)} {Currency}";
    }

    // Operator overloads for convenience
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);
    public static bool operator >(Money left, Money right) => left.IsGreaterThan(right);
    public static bool operator <(Money left, Money right) => left.IsLessThan(right);
    public static bool operator >=(Money left, Money right) => left.IsGreaterThanOrEqual(right);
    public static bool operator <=(Money left, Money right) => left.IsLessThanOrEqual(right);
}
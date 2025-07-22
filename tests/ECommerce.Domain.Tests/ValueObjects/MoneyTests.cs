using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidAmountAndCurrency_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "USD";

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithLowercaseCurrency_ShouldConvertToUppercase()
    {
        // Arrange
        var amount = 100m;
        var currency = "usd";

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyCurrency_ShouldThrowArgumentException(string currency)
    {
        // Arrange
        var amount = 100m;

        // Act & Assert
        var action = () => new Money(amount, currency);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Currency cannot be null or empty.*");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("A")]
    public void Constructor_WithInvalidCurrencyLength_ShouldThrowArgumentException(string currency)
    {
        // Arrange
        var amount = 100m;

        // Act & Assert
        var action = () => new Money(amount, currency);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Currency must be a 3-character ISO code.*");
    }

    [Fact]
    public void Zero_WithValidCurrency_ShouldCreateZeroMoney()
    {
        // Arrange
        var currency = "EUR";

        // Act
        var money = Money.Zero(currency);

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSummedMoney()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act & Assert
        var action = () => money1.Add(money2);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot perform operation on different currencies: USD and EUR");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifferenceMoney()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "EUR");

        // Act & Assert
        var action = () => money1.Subtract(money2);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiply_WithPositiveFactor_ShouldReturnMultipliedMoney()
    {
        // Arrange
        var money = new Money(100m, "USD");
        var factor = 2.5m;

        // Act
        var result = money.Multiply(factor);

        // Assert
        result.Amount.Should().Be(250m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Divide_WithValidDivisor_ShouldReturnDividedMoney()
    {
        // Arrange
        var money = new Money(100m, "USD");
        var divisor = 4m;

        // Act
        var result = money.Divide(divisor);

        // Assert
        result.Amount.Should().Be(25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Divide_WithZeroDivisor_ShouldThrowDivideByZeroException()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act & Assert
        var action = () => money.Divide(0);
        action.Should().Throw<DivideByZeroException>()
            .WithMessage("Cannot divide money by zero.");
    }

    [Fact]
    public void IsGreaterThan_WithSmallerAmount_ShouldReturnTrue()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1.IsGreaterThan(money2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLessThan_WithLargerAmount_ShouldReturnTrue()
    {
        // Arrange
        var money1 = new Money(50m, "USD");
        var money2 = new Money(100m, "USD");

        // Act
        var result = money1.IsLessThan(money2);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-50, false)]
    public void IsPositive_ShouldReturnCorrectValue(decimal amount, bool expected)
    {
        // Arrange
        var money = new Money(amount, "USD");

        // Act
        var result = money.IsPositive;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-100, true)]
    [InlineData(0, false)]
    [InlineData(50, false)]
    public void IsNegative_ShouldReturnCorrectValue(decimal amount, bool expected)
    {
        // Arrange
        var money = new Money(amount, "USD");

        // Act
        var result = money.IsNegative;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(100, false)]
    [InlineData(-50, false)]
    public void IsZero_ShouldReturnCorrectValue(decimal amount, bool expected)
    {
        // Arrange
        var money = new Money(amount, "USD");

        // Act
        var result = money.IsZero;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void OperatorAdd_WithSameCurrency_ShouldReturnSummedMoney()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void OperatorSubtract_WithSameCurrency_ShouldReturnDifferenceMoney()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "USD");

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(70m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void OperatorMultiply_ShouldReturnMultipliedMoney()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money * 2m;

        // Assert
        result.Amount.Should().Be(200m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void OperatorDivide_ShouldReturnDividedMoney()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money / 4m;

        // Assert
        result.Amount.Should().Be(25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void OperatorGreaterThan_ShouldReturnCorrectComparison()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act & Assert
        (money1 > money2).Should().BeTrue();
        (money2 > money1).Should().BeFalse();
    }

    [Fact]
    public void OperatorLessThan_ShouldReturnCorrectComparison()
    {
        // Arrange
        var money1 = new Money(50m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        (money1 < money2).Should().BeTrue();
        (money2 < money1).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = new Money(123.45m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("123.45 USD");
    }

    [Fact]
    public void Equality_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentAmountOrCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");
        var money3 = new Money(50m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        money1.Should().NotBe(money3);
        (money1 == money2).Should().BeFalse();
        (money1 == money3).Should().BeFalse();
    }
}
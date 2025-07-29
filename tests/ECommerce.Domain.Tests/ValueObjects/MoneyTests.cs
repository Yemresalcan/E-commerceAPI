using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateMoney()
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
    public void Constructor_WithLowerCaseCurrency_ShouldConvertToUpperCase()
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
    public void Constructor_WithInvalidCurrency_ShouldThrowArgumentException(string currency)
    {
        // Arrange
        var amount = 100m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Money(amount, currency));
        exception.Message.Should().Contain("Currency cannot be null or empty");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U")]
    public void Constructor_WithInvalidCurrencyLength_ShouldThrowArgumentException(string currency)
    {
        // Arrange
        var amount = 100m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Money(amount, currency));
        exception.Message.Should().Contain("Currency must be a 3-character ISO code");
    }

    [Fact]
    public void Zero_ShouldCreateZeroMoney()
    {
        // Arrange
        var currency = "USD";

        // Act
        var money = Money.Zero(currency);

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
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
        var exception = Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
        exception.Message.Should().Contain("Cannot perform operation on different currencies");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
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
        var exception = Assert.Throws<InvalidOperationException>(() => money1.Subtract(money2));
        exception.Message.Should().Contain("Cannot perform operation on different currencies");
    }

    [Fact]
    public void Multiply_ShouldReturnProduct()
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
    public void Divide_WithNonZeroDivisor_ShouldReturnQuotient()
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
        Assert.Throws<DivideByZeroException>(() => money.Divide(0));
    }

    [Fact]
    public void IsGreaterThan_WithSameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act & Assert
        money1.IsGreaterThan(money2).Should().BeTrue();
        money2.IsGreaterThan(money1).Should().BeFalse();
    }

    [Fact]
    public void IsLessThan_WithSameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var money1 = new Money(50m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        money1.IsLessThan(money2).Should().BeTrue();
        money2.IsLessThan(money1).Should().BeFalse();
    }

    [Fact]
    public void IsGreaterThanOrEqual_WithSameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");
        var money3 = new Money(50m, "USD");

        // Act & Assert
        money1.IsGreaterThanOrEqual(money2).Should().BeTrue();
        money1.IsGreaterThanOrEqual(money3).Should().BeTrue();
        money3.IsGreaterThanOrEqual(money1).Should().BeFalse();
    }

    [Fact]
    public void IsLessThanOrEqual_WithSameCurrency_ShouldReturnCorrectResult()
    {
        // Arrange
        var money1 = new Money(50m, "USD");
        var money2 = new Money(50m, "USD");
        var money3 = new Money(100m, "USD");

        // Act & Assert
        money1.IsLessThanOrEqual(money2).Should().BeTrue();
        money1.IsLessThanOrEqual(money3).Should().BeTrue();
        money3.IsLessThanOrEqual(money1).Should().BeFalse();
    }

    [Fact]
    public void IsPositive_ShouldReturnCorrectResult()
    {
        // Arrange
        var positiveMoney = new Money(100m, "USD");
        var negativeMoney = new Money(-100m, "USD");
        var zeroMoney = Money.Zero("USD");

        // Act & Assert
        positiveMoney.IsPositive.Should().BeTrue();
        negativeMoney.IsPositive.Should().BeFalse();
        zeroMoney.IsPositive.Should().BeFalse();
    }

    [Fact]
    public void IsNegative_ShouldReturnCorrectResult()
    {
        // Arrange
        var positiveMoney = new Money(100m, "USD");
        var negativeMoney = new Money(-100m, "USD");
        var zeroMoney = Money.Zero("USD");

        // Act & Assert
        positiveMoney.IsNegative.Should().BeFalse();
        negativeMoney.IsNegative.Should().BeTrue();
        zeroMoney.IsNegative.Should().BeFalse();
    }

    [Fact]
    public void IsZero_ShouldReturnCorrectResult()
    {
        // Arrange
        var positiveMoney = new Money(100m, "USD");
        var zeroMoney = Money.Zero("USD");

        // Act & Assert
        positiveMoney.IsZero.Should().BeFalse();
        zeroMoney.IsZero.Should().BeTrue();
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
    public void OperatorOverloads_ShouldWorkCorrectly()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act & Assert
        (money1 + money2).Amount.Should().Be(150m);
        (money1 - money2).Amount.Should().Be(50m);
        (money1 * 2).Amount.Should().Be(200m);
        (money1 / 2).Amount.Should().Be(50m);
        (money1 > money2).Should().BeTrue();
        (money1 < money2).Should().BeFalse();
        (money1 >= money2).Should().BeTrue();
        (money1 <= money2).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
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
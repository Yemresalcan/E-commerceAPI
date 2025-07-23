using ECommerce.Domain.Aggregates.OrderAggregate;
using ECommerce.Domain.Exceptions;
using FluentAssertions;

namespace ECommerce.Domain.Tests.Aggregates.OrderAggregate;

public class PaymentTests
{
    private const decimal ValidAmount = 100.00m;
    private const string ValidCurrency = "USD";
    private const PaymentMethod ValidMethod = PaymentMethod.CreditCard;
    private const string ValidProvider = "Stripe";
    private const string ValidTransactionId = "txn_123456789";

    [Fact]
    public void Create_WithValidData_ShouldCreatePayment()
    {
        // Act
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod, ValidProvider);

        // Assert
        payment.Should().NotBeNull();
        payment.Id.Should().NotBeEmpty();
        payment.Amount.Should().Be(ValidAmount);
        payment.Currency.Should().Be(ValidCurrency);
        payment.Method.Should().Be(ValidMethod);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.PaymentProvider.Should().Be(ValidProvider);
        payment.TransactionId.Should().BeNull();
        payment.ProcessedAt.Should().BeNull();
        payment.FailureReason.Should().BeNull();
        payment.IsInFinalState.Should().BeFalse();
        payment.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void Create_WithoutProvider_ShouldCreatePayment()
    {
        // Act
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);

        // Assert
        payment.PaymentProvider.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrowException()
    {
        // Act & Assert
        var act = () => Payment.Create(0, ValidCurrency, ValidMethod);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Payment amount must be greater than zero");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act & Assert
        var act = () => Payment.Create(-50.00m, ValidCurrency, ValidMethod);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Payment amount must be greater than zero");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrowException()
    {
        // Act & Assert
        var act = () => Payment.Create(ValidAmount, "", ValidMethod);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Payment currency is required");
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldThrowException()
    {
        // Act & Assert
        var act = () => Payment.Create(ValidAmount, null!, ValidMethod);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Payment currency is required");
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ShouldThrowException()
    {
        // Act & Assert
        var act = () => Payment.Create(ValidAmount, "USDD", ValidMethod);
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Currency must be a 3-character ISO code");
    }

    [Fact]
    public void Create_WithLowercaseCurrency_ShouldConvertToUppercase()
    {
        // Act
        var payment = Payment.Create(ValidAmount, "usd", ValidMethod);

        // Assert
        payment.Currency.Should().Be("USD");
    }

    [Fact]
    public void MarkAsProcessing_FromPending_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);

        // Act
        payment.MarkAsProcessing(ValidTransactionId);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.TransactionId.Should().Be(ValidTransactionId);
        payment.IsInFinalState.Should().BeFalse();
    }

    [Fact]
    public void MarkAsProcessing_WithoutTransactionId_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);

        // Act
        payment.MarkAsProcessing();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.TransactionId.Should().BeNull();
    }

    [Fact]
    public void MarkAsProcessing_FromNonPendingStatus_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsProcessing();

        // Act & Assert
        var act = () => payment.MarkAsProcessing();
        act.Should().Throw<InvalidPaymentStateException>()
            .WithMessage("Cannot mark as processing payment in Processing state");
    }

    [Fact]
    public void MarkAsCompleted_FromProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsProcessing();

        // Act
        payment.MarkAsCompleted(ValidTransactionId);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().Be(ValidTransactionId);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.FailureReason.Should().BeNull();
        payment.IsInFinalState.Should().BeTrue();
        payment.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void MarkAsCompleted_WithEmptyTransactionId_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsProcessing();

        // Act & Assert
        var act = () => payment.MarkAsCompleted("");
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Transaction ID is required for completed payment");
    }

    [Fact]
    public void MarkAsCompleted_FromNonProcessingStatus_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);

        // Act & Assert
        var act = () => payment.MarkAsCompleted(ValidTransactionId);
        act.Should().Throw<InvalidPaymentStateException>()
            .WithMessage("Cannot complete payment in Pending state");
    }

    [Fact]
    public void MarkAsFailed_FromPending_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        var failureReason = "Insufficient funds";

        // Act
        payment.MarkAsFailed(failureReason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be(failureReason);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.IsInFinalState.Should().BeTrue();
        payment.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void MarkAsFailed_FromProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsProcessing();
        var failureReason = "Card declined";

        // Act
        payment.MarkAsFailed(failureReason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be(failureReason);
    }

    [Fact]
    public void MarkAsFailed_FromCompleted_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsProcessing();
        payment.MarkAsCompleted(ValidTransactionId);

        // Act & Assert
        var act = () => payment.MarkAsFailed("Some reason");
        act.Should().Throw<InvalidPaymentStateException>()
            .WithMessage("Cannot mark as failed payment in Completed state");
    }

    [Fact]
    public void MarkAsFailed_WithEmptyReason_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);

        // Act & Assert
        var act = () => payment.MarkAsFailed("");
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Failure reason is required for failed payment");
    }

    [Fact]
    public void MarkAsRefunded_FromCompleted_ShouldUpdateStatus()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsProcessing();
        payment.MarkAsCompleted(ValidTransactionId);
        var refundTransactionId = "refund_123456789";

        // Act
        payment.MarkAsRefunded(refundTransactionId);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.TransactionId.Should().Be(refundTransactionId);
        payment.IsInFinalState.Should().BeTrue();
        payment.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void MarkAsRefunded_FromNonCompletedStatus_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);

        // Act & Assert
        var act = () => payment.MarkAsRefunded("refund_123");
        act.Should().Throw<InvalidPaymentStateException>()
            .WithMessage("Cannot refund payment in Pending state");
    }

    [Fact]
    public void MarkAsRefunded_WithEmptyTransactionId_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsProcessing();
        payment.MarkAsCompleted(ValidTransactionId);

        // Act & Assert
        var act = () => payment.MarkAsRefunded("");
        act.Should().Throw<OrderDomainException>()
            .WithMessage("Refund transaction ID is required");
    }

    [Fact]
    public void Retry_FromFailed_ShouldResetTopending()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsFailed("Card declined");

        // Act
        payment.Retry();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.FailureReason.Should().BeNull();
        payment.TransactionId.Should().BeNull();
        payment.ProcessedAt.Should().BeNull();
        payment.IsInFinalState.Should().BeFalse();
        payment.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void Retry_FromNonFailedStatus_ShouldThrowException()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);

        // Act & Assert
        var act = () => payment.Retry();
        act.Should().Throw<InvalidPaymentStateException>()
            .WithMessage("Cannot retry payment in Pending state");
    }

    // Note: SetOrderId is an internal method used by the Order aggregate
    // and doesn't need to be tested directly as it's tested through Order aggregate tests

    [Theory]
    [InlineData(PaymentStatus.Completed, true)]
    [InlineData(PaymentStatus.Failed, true)]
    [InlineData(PaymentStatus.Refunded, true)]
    [InlineData(PaymentStatus.Pending, false)]
    [InlineData(PaymentStatus.Processing, false)]
    public void IsInFinalState_ShouldReturnCorrectValue(PaymentStatus status, bool expectedResult)
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        
        // Set the status based on the test case
        switch (status)
        {
            case PaymentStatus.Processing:
                payment.MarkAsProcessing();
                break;
            case PaymentStatus.Completed:
                payment.MarkAsProcessing();
                payment.MarkAsCompleted(ValidTransactionId);
                break;
            case PaymentStatus.Failed:
                payment.MarkAsFailed("Test failure");
                break;
            case PaymentStatus.Refunded:
                payment.MarkAsProcessing();
                payment.MarkAsCompleted(ValidTransactionId);
                payment.MarkAsRefunded("refund_123");
                break;
        }

        // Act & Assert
        payment.IsInFinalState.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(PaymentStatus.Completed, true)]
    [InlineData(PaymentStatus.Failed, false)]
    [InlineData(PaymentStatus.Refunded, false)]
    [InlineData(PaymentStatus.Pending, false)]
    [InlineData(PaymentStatus.Processing, false)]
    public void IsSuccessful_ShouldReturnCorrectValue(PaymentStatus status, bool expectedResult)
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        
        // Set the status based on the test case
        switch (status)
        {
            case PaymentStatus.Processing:
                payment.MarkAsProcessing();
                break;
            case PaymentStatus.Completed:
                payment.MarkAsProcessing();
                payment.MarkAsCompleted(ValidTransactionId);
                break;
            case PaymentStatus.Failed:
                payment.MarkAsFailed("Test failure");
                break;
            case PaymentStatus.Refunded:
                payment.MarkAsProcessing();
                payment.MarkAsCompleted(ValidTransactionId);
                payment.MarkAsRefunded("refund_123");
                break;
        }

        // Act & Assert
        payment.IsSuccessful.Should().Be(expectedResult);
    }

    [Fact]
    public void PaymentLifecycle_HappyPath_ShouldWorkCorrectly()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod, ValidProvider);

        // Act & Assert - Pending to Processing
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.MarkAsProcessing(ValidTransactionId);
        payment.Status.Should().Be(PaymentStatus.Processing);

        // Act & Assert - Processing to Completed
        payment.MarkAsCompleted(ValidTransactionId);
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.IsSuccessful.Should().BeTrue();
        payment.IsInFinalState.Should().BeTrue();
    }

    [Fact]
    public void PaymentLifecycle_FailureAndRetry_ShouldWorkCorrectly()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod, ValidProvider);

        // Act & Assert - Pending to Failed
        payment.MarkAsFailed("Card declined");
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.IsSuccessful.Should().BeFalse();
        payment.IsInFinalState.Should().BeTrue();

        // Act & Assert - Failed to Pending (retry)
        payment.Retry();
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.IsInFinalState.Should().BeFalse();

        // Act & Assert - Complete the retry
        payment.MarkAsProcessing();
        payment.MarkAsCompleted(ValidTransactionId);
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void PaymentLifecycle_RefundAfterCompletion_ShouldWorkCorrectly()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod, ValidProvider);

        // Act & Assert - Complete payment
        payment.MarkAsProcessing();
        payment.MarkAsCompleted(ValidTransactionId);
        payment.Status.Should().Be(PaymentStatus.Completed);

        // Act & Assert - Refund payment
        var refundTransactionId = "refund_123456789";
        payment.MarkAsRefunded(refundTransactionId);
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.TransactionId.Should().Be(refundTransactionId);
        payment.IsSuccessful.Should().BeFalse();
        payment.IsInFinalState.Should().BeTrue();
    }

    [Theory]
    [InlineData(PaymentMethod.CreditCard)]
    [InlineData(PaymentMethod.DebitCard)]
    [InlineData(PaymentMethod.PayPal)]
    [InlineData(PaymentMethod.BankTransfer)]
    [InlineData(PaymentMethod.DigitalWallet)]
    [InlineData(PaymentMethod.CashOnDelivery)]
    public void Create_WithDifferentPaymentMethods_ShouldCreatePayment(PaymentMethod method)
    {
        // Act
        var payment = Payment.Create(ValidAmount, ValidCurrency, method);

        // Assert
        payment.Method.Should().Be(method);
    }

    [Fact]
    public void MarkAsCompleted_ShouldClearPreviousFailureReason()
    {
        // Arrange
        var payment = Payment.Create(ValidAmount, ValidCurrency, ValidMethod);
        payment.MarkAsFailed("Initial failure");
        payment.Retry();
        payment.MarkAsProcessing();

        // Act
        payment.MarkAsCompleted(ValidTransactionId);

        // Assert
        payment.FailureReason.Should().BeNull();
        payment.Status.Should().Be(PaymentStatus.Completed);
    }
}
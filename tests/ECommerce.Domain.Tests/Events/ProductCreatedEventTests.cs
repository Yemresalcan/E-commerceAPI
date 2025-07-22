using ECommerce.Domain.Events;

namespace ECommerce.Domain.Tests.Events;

public class ProductCreatedEventTests
{
    [Fact]
    public void ProductCreatedEvent_Should_Initialize_All_Properties_Correctly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var name = "Test Product";
        var priceAmount = 99.99m;
        var currency = "USD";
        var categoryId = Guid.NewGuid();
        var stockQuantity = 100;

        // Act
        var productCreatedEvent = new ProductCreatedEvent(
            productId,
            name,
            priceAmount,
            currency,
            categoryId,
            stockQuantity);

        // Assert
        productCreatedEvent.ProductId.Should().Be(productId);
        productCreatedEvent.Name.Should().Be(name);
        productCreatedEvent.PriceAmount.Should().Be(priceAmount);
        productCreatedEvent.Currency.Should().Be(currency);
        productCreatedEvent.CategoryId.Should().Be(categoryId);
        productCreatedEvent.StockQuantity.Should().Be(stockQuantity);
        productCreatedEvent.Id.Should().NotBe(Guid.Empty);
        productCreatedEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        productCreatedEvent.Version.Should().Be(1);
    }

    [Fact]
    public void ProductCreatedEvent_Should_Inherit_From_DomainEvent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        // Act
        var productCreatedEvent = new ProductCreatedEvent(
            productId,
            "Test Product",
            99.99m,
            "USD",
            categoryId,
            100);

        // Assert
        productCreatedEvent.Should().BeAssignableTo<DomainEvent>();
    }

    [Fact]
    public void ProductCreatedEvent_Should_Support_Record_Equality()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var event1 = new ProductCreatedEvent(
            productId,
            "Test Product",
            99.99m,
            "USD",
            categoryId,
            100);

        var event2 = new ProductCreatedEvent(
            productId,
            "Test Product",
            99.99m,
            "USD",
            categoryId,
            100);

        // Act & Assert
        event1.Should().NotBe(event2); // Different instances have different IDs and OccurredOn
        event1.ProductId.Should().Be(event2.ProductId);
        event1.Name.Should().Be(event2.Name);
        event1.PriceAmount.Should().Be(event2.PriceAmount);
        event1.Currency.Should().Be(event2.Currency);
        event1.CategoryId.Should().Be(event2.CategoryId);
        event1.StockQuantity.Should().Be(event2.StockQuantity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Product Name")]
    [InlineData("Very Long Product Name With Special Characters !@#$%")]
    public void ProductCreatedEvent_Should_Accept_Various_Product_Names(string productName)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        // Act
        var productCreatedEvent = new ProductCreatedEvent(
            productId,
            productName,
            99.99m,
            "USD",
            categoryId,
            100);

        // Assert
        productCreatedEvent.Name.Should().Be(productName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void ProductCreatedEvent_Should_Accept_Various_Stock_Quantities(int stockQuantity)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        // Act
        var productCreatedEvent = new ProductCreatedEvent(
            productId,
            "Test Product",
            99.99m,
            "USD",
            categoryId,
            stockQuantity);

        // Assert
        productCreatedEvent.StockQuantity.Should().Be(stockQuantity);
    }
}
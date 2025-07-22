using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Events;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ECommerce.Domain.Tests.Aggregates.ProductAggregate;

public class ProductTests
{
    private readonly Money _validPrice = new(100.00m, "USD");
    private readonly Guid _categoryId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_ShouldCreateProduct()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var sku = "TEST-001";
        var stockQuantity = 10;
        var minimumStockLevel = 5;
        var weight = 1.5m;
        var dimensions = "10x10x10";

        // Act
        var product = Product.Create(name, description, _validPrice, sku, stockQuantity, minimumStockLevel, _categoryId, weight, dimensions);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBe(Guid.Empty);
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(_validPrice);
        product.Sku.Should().Be(sku);
        product.StockQuantity.Should().Be(stockQuantity);
        product.MinimumStockLevel.Should().Be(minimumStockLevel);
        product.CategoryId.Should().Be(_categoryId);
        product.Weight.Should().Be(weight);
        product.Dimensions.Should().Be(dimensions);
        product.IsActive.Should().BeTrue();
        product.IsFeatured.Should().BeFalse();
        product.IsInStock.Should().BeTrue();
        product.IsLowStock.Should().BeFalse();
        product.IsOutOfStock.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidParameters_ShouldRaiseProductCreatedEvent()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var sku = "TEST-001";

        // Act
        var product = Product.Create(name, description, _validPrice, sku, 10, 5, _categoryId);

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>()
            .Which.Should().Match<ProductCreatedEvent>(e =>
                e.ProductId == product.Id &&
                e.Name == name &&
                e.PriceAmount == _validPrice.Amount &&
                e.Currency == _validPrice.Currency &&
                e.CategoryId == _categoryId &&
                e.StockQuantity == 10);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        var act = () => Product.Create(invalidName, "Description", _validPrice, "SKU", 10, 5, _categoryId);
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('a', 256);

        // Act & Assert
        var act = () => Product.Create(longName, "Description", _validPrice, "SKU", 10, 5, _categoryId);
        act.Should().Throw<ArgumentException>().WithMessage("*name*exceed*255*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Act & Assert
        var act = () => Product.Create("Name", invalidDescription, _validPrice, "SKU", 10, 5, _categoryId);
        act.Should().Throw<ArgumentException>().WithMessage("*description*");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowInvalidProductPriceException()
    {
        // Arrange
        var negativePrice = new Money(-10.00m, "USD");

        // Act & Assert
        var act = () => Product.Create("Name", "Description", negativePrice, "SKU", 10, 5, _categoryId);
        act.Should().Throw<InvalidProductPriceException>();
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldThrowInvalidProductPriceException()
    {
        // Arrange
        var zeroPrice = new Money(0.00m, "USD");

        // Act & Assert
        var act = () => Product.Create("Name", "Description", zeroPrice, "SKU", 10, 5, _categoryId);
        act.Should().Throw<InvalidProductPriceException>();
    }

    [Fact]
    public void Create_WithNegativeStock_ShouldThrowInvalidStockQuantityException()
    {
        // Act & Assert
        var act = () => Product.Create("Name", "Description", _validPrice, "SKU", -1, 5, _categoryId);
        act.Should().Throw<InvalidStockQuantityException>();
    }

    [Fact]
    public void Create_WithNegativeMinimumStock_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => Product.Create("Name", "Description", _validPrice, "SKU", 10, -1, _categoryId);
        act.Should().Throw<ArgumentException>().WithMessage("*Minimum stock level*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateProduct()
    {
        // Arrange
        var product = CreateValidProduct();
        var newName = "Updated Product";
        var newDescription = "Updated Description";
        var newPrice = new Money(200.00m, "USD");
        var newWeight = 2.5m;
        var newDimensions = "20x20x20";

        // Act
        product.Update(newName, newDescription, newPrice, newWeight, newDimensions);

        // Assert
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
        product.Weight.Should().Be(newWeight);
        product.Dimensions.Should().Be(newDimensions);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldRaiseProductUpdatedEvent()
    {
        // Arrange
        var product = CreateValidProduct();
        product.ClearDomainEvents(); // Clear creation event
        var newName = "Updated Product";
        var newPrice = new Money(200.00m, "USD");

        // Act
        product.Update(newName, "Updated Description", newPrice);

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdatedEvent>()
            .Which.Should().Match<ProductUpdatedEvent>(e =>
                e.ProductId == product.Id &&
                e.Name == newName &&
                e.PriceAmount == newPrice.Amount &&
                e.Currency == newPrice.Currency);
    }

    [Fact]
    public void IncreaseStock_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = CreateValidProduct();
        var initialStock = product.StockQuantity;
        var increaseAmount = 5;

        // Act
        product.IncreaseStock(increaseAmount, "Restock");

        // Assert
        product.StockQuantity.Should().Be(initialStock + increaseAmount);
    }

    [Fact]
    public void IncreaseStock_WithValidQuantity_ShouldRaiseStockUpdatedEvent()
    {
        // Arrange
        var product = CreateValidProduct();
        product.ClearDomainEvents();
        var initialStock = product.StockQuantity;
        var increaseAmount = 5;

        // Act
        product.IncreaseStock(increaseAmount, "Restock");

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductStockUpdatedEvent>()
            .Which.Should().Match<ProductStockUpdatedEvent>(e =>
                e.ProductId == product.Id &&
                e.PreviousStock == initialStock &&
                e.NewStock == initialStock + increaseAmount &&
                e.Reason == "Restock");
    }

    [Fact]
    public void DecreaseStock_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var product = CreateValidProduct();
        var initialStock = product.StockQuantity;
        var decreaseAmount = 3;

        // Act
        product.DecreaseStock(decreaseAmount, "Sale");

        // Assert
        product.StockQuantity.Should().Be(initialStock - decreaseAmount);
    }

    [Fact]
    public void DecreaseStock_WithQuantityGreaterThanStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var product = CreateValidProduct();
        var excessiveAmount = product.StockQuantity + 1;

        // Act & Assert
        var act = () => product.DecreaseStock(excessiveAmount);
        act.Should().Throw<InsufficientStockException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncreaseStock_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        // Arrange
        var product = CreateValidProduct();

        // Act & Assert
        var act = () => product.IncreaseStock(invalidQuantity);
        act.Should().Throw<ArgumentException>().WithMessage("*Quantity must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecreaseStock_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        // Arrange
        var product = CreateValidProduct();

        // Act & Assert
        var act = () => product.DecreaseStock(invalidQuantity);
        act.Should().Throw<ArgumentException>().WithMessage("*Quantity must be positive*");
    }

    [Fact]
    public void SetStock_WithValidQuantity_ShouldSetStock()
    {
        // Arrange
        var product = CreateValidProduct();
        var newStock = 25;

        // Act
        product.SetStock(newStock, "Inventory adjustment");

        // Assert
        product.StockQuantity.Should().Be(newStock);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var product = CreateValidProduct();
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void MarkAsFeatured_ShouldSetIsFeaturedToTrue()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        product.MarkAsFeatured();

        // Assert
        product.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public void RemoveFeaturedStatus_ShouldSetIsFeaturedToFalse()
    {
        // Arrange
        var product = CreateValidProduct();
        product.MarkAsFeatured();

        // Act
        product.RemoveFeaturedStatus();

        // Assert
        product.IsFeatured.Should().BeFalse();
    }

    [Fact]
    public void AddReview_WithValidParameters_ShouldAddReview()
    {
        // Arrange
        var product = CreateValidProduct();
        var customerId = Guid.NewGuid();
        var rating = 5;
        var title = "Great product!";
        var content = "I really love this product.";

        // Act
        product.AddReview(customerId, rating, title, content, true);

        // Assert
        product.Reviews.Should().ContainSingle();
        var review = product.Reviews.First();
        review.CustomerId.Should().Be(customerId);
        review.Rating.Should().Be(rating);
        review.Title.Should().Be(title);
        review.Content.Should().Be(content);
        review.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void AddReview_WithValidParameters_ShouldRaiseProductReviewAddedEvent()
    {
        // Arrange
        var product = CreateValidProduct();
        product.ClearDomainEvents();
        var customerId = Guid.NewGuid();

        // Act
        product.AddReview(customerId, 5, "Title", "Content", true);

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductReviewAddedEvent>()
            .Which.Should().Match<ProductReviewAddedEvent>(e =>
                e.ProductId == product.Id &&
                e.CustomerId == customerId &&
                e.Rating == 5 &&
                e.IsVerified == true);
    }

    [Fact]
    public void AddReview_WhenCustomerAlreadyReviewed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var product = CreateValidProduct();
        var customerId = Guid.NewGuid();
        product.AddReview(customerId, 5, "Title", "Content");

        // Act & Assert
        var act = () => product.AddReview(customerId, 4, "Another Title", "Another Content");
        act.Should().Throw<InvalidOperationException>().WithMessage("*already reviewed*");
    }

    [Fact]
    public void RemoveReview_WithExistingReview_ShouldRemoveReview()
    {
        // Arrange
        var product = CreateValidProduct();
        var customerId = Guid.NewGuid();
        product.AddReview(customerId, 5, "Title", "Content");
        var reviewId = product.Reviews.First().Id;

        // Act
        product.RemoveReview(reviewId);

        // Assert
        product.Reviews.Should().BeEmpty();
    }

    [Fact]
    public void CanFulfillOrder_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateValidProduct();
        var requestedQuantity = product.StockQuantity - 1;

        // Act
        var result = product.CanFulfillOrder(requestedQuantity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanFulfillOrder_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var product = CreateValidProduct();
        var requestedQuantity = product.StockQuantity + 1;

        // Act
        var result = product.CanFulfillOrder(requestedQuantity);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanFulfillOrder_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var product = CreateValidProduct();
        product.Deactivate();

        // Act
        var result = product.CanFulfillOrder(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLowStock_WhenStockAtMinimumLevel_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateValidProduct();
        product.SetStock(product.MinimumStockLevel);

        // Act & Assert
        product.IsLowStock.Should().BeTrue();
    }

    [Fact]
    public void IsOutOfStock_WhenStockIsZero_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateValidProduct();
        product.SetStock(0);

        // Act & Assert
        product.IsOutOfStock.Should().BeTrue();
        product.IsInStock.Should().BeFalse();
    }

    [Fact]
    public void AverageRating_WithApprovedReviews_ShouldCalculateCorrectly()
    {
        // Arrange
        var product = CreateValidProduct();
        product.AddReview(Guid.NewGuid(), 5, "Title1", "Content1");
        product.AddReview(Guid.NewGuid(), 3, "Title2", "Content2");
        
        // Approve the reviews
        var reviews = product.Reviews.ToList();
        reviews[0].Approve();
        reviews[1].Approve();

        // Act
        var averageRating = product.AverageRating;

        // Assert
        averageRating.Should().Be(4.0m); // (5 + 3) / 2 = 4
    }

    [Fact]
    public void ReviewCount_WithApprovedReviews_ShouldReturnCorrectCount()
    {
        // Arrange
        var product = CreateValidProduct();
        product.AddReview(Guid.NewGuid(), 5, "Title1", "Content1");
        product.AddReview(Guid.NewGuid(), 3, "Title2", "Content2");
        
        // Approve only one review
        product.Reviews.First().Approve();

        // Act
        var reviewCount = product.ReviewCount;

        // Assert
        reviewCount.Should().Be(1);
    }

    private Product CreateValidProduct()
    {
        return Product.Create(
            "Test Product",
            "Test Description",
            _validPrice,
            "TEST-001",
            10,
            5,
            _categoryId);
    }
}
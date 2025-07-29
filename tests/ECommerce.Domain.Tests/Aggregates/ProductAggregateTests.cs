using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Events;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Tests.Aggregates;

public class ProductAggregateTests
{
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Money _validPrice = new(100m, "USD");

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
        var product = Product.Create(
            name, description, _validPrice, sku, stockQuantity, 
            minimumStockLevel, _categoryId, weight, dimensions);

        // Assert
        product.Should().NotBeNull();
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
    }

    [Fact]
    public void Create_ShouldRaiseProductCreatedEvent()
    {
        // Act
        var product = Product.Create(
            "Test Product", "Description", _validPrice, "TEST-001", 
            10, 5, _categoryId);

        // Assert
        var domainEvent = product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>().Subject;
        
        domainEvent.ProductId.Should().Be(product.Id);
        domainEvent.Name.Should().Be("Test Product");
        domainEvent.PriceAmount.Should().Be(100m);
        domainEvent.Currency.Should().Be("USD");
        domainEvent.CategoryId.Should().Be(_categoryId);
        domainEvent.StockQuantity.Should().Be(10);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create(invalidName, "Description", _validPrice, "TEST-001", 10, 5, _categoryId));
        
        exception.Message.Should().Contain("Product name cannot be null or empty");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('a', 256);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create(longName, "Description", _validPrice, "TEST-001", 10, 5, _categoryId));
        
        exception.Message.Should().Contain("Product name cannot exceed 255 characters");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create("Test Product", invalidDescription, _validPrice, "TEST-001", 10, 5, _categoryId));
        
        exception.Message.Should().Contain("Product description cannot be null or empty");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longDescription = new string('a', 2001);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create("Test Product", longDescription, _validPrice, "TEST-001", 10, 5, _categoryId));
        
        exception.Message.Should().Contain("Product description cannot exceed 2000 characters");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowInvalidProductPriceException()
    {
        // Arrange
        var negativePrice = new Money(-10m, "USD");

        // Act & Assert
        Assert.Throws<InvalidProductPriceException>(() =>
            Product.Create("Test Product", "Description", negativePrice, "TEST-001", 10, 5, _categoryId));
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldThrowInvalidProductPriceException()
    {
        // Arrange
        var zeroPrice = Money.Zero("USD");

        // Act & Assert
        Assert.Throws<InvalidProductPriceException>(() =>
            Product.Create("Test Product", "Description", zeroPrice, "TEST-001", 10, 5, _categoryId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSku_ShouldThrowArgumentException(string invalidSku)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create("Test Product", "Description", _validPrice, invalidSku, 10, 5, _categoryId));
        
        exception.Message.Should().Contain("Product SKU cannot be null or empty");
    }

    [Fact]
    public void Create_WithSkuTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longSku = new string('a', 51);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create("Test Product", "Description", _validPrice, longSku, 10, 5, _categoryId));
        
        exception.Message.Should().Contain("Product SKU cannot exceed 50 characters");
    }

    [Fact]
    public void Create_WithNegativeStockQuantity_ShouldThrowInvalidStockQuantityException()
    {
        // Act & Assert
        Assert.Throws<InvalidStockQuantityException>(() =>
            Product.Create("Test Product", "Description", _validPrice, "TEST-001", -1, 5, _categoryId));
    }

    [Fact]
    public void Create_WithNegativeMinimumStockLevel_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create("Test Product", "Description", _validPrice, "TEST-001", 10, -1, _categoryId));
        
        exception.Message.Should().Contain("Minimum stock level cannot be negative");
    }

    [Fact]
    public void Create_WithNegativeWeight_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Product.Create("Test Product", "Description", _validPrice, "TEST-001", 10, 5, _categoryId, -1m));
        
        exception.Message.Should().Contain("Product weight cannot be negative");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateProduct()
    {
        // Arrange
        var product = Product.Create("Original", "Original Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var newName = "Updated Product";
        var newDescription = "Updated Description";
        var newPrice = new Money(200m, "USD");
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
    public void Update_ShouldRaiseProductUpdatedEvent()
    {
        // Arrange
        var product = Product.Create("Original", "Original Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        product.ClearDomainEvents(); // Clear creation event
        var newPrice = new Money(200m, "USD");

        // Act
        product.Update("Updated", "Updated Description", newPrice);

        // Assert
        var domainEvent = product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdatedEvent>().Subject;
        
        domainEvent.ProductId.Should().Be(product.Id);
        domainEvent.Name.Should().Be("Updated");
        domainEvent.PriceAmount.Should().Be(200m);
        domainEvent.Currency.Should().Be("USD");
    }

    [Fact]
    public void UpdateCategory_WithValidCategoryId_ShouldUpdateCategory()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var newCategoryId = Guid.NewGuid();

        // Act
        product.UpdateCategory(newCategoryId);

        // Assert
        product.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public void UpdateCategory_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => product.UpdateCategory(Guid.Empty));
        exception.Message.Should().Contain("Category ID cannot be empty");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
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
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void MarkAsFeatured_ShouldSetIsFeaturedToTrue()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act
        product.MarkAsFeatured();

        // Assert
        product.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public void RemoveFeaturedStatus_ShouldSetIsFeaturedToFalse()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        product.MarkAsFeatured();

        // Act
        product.RemoveFeaturedStatus();

        // Assert
        product.IsFeatured.Should().BeFalse();
    }

    [Fact]
    public void IncreaseStock_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var initialStock = product.StockQuantity;

        // Act
        product.IncreaseStock(5);

        // Assert
        product.StockQuantity.Should().Be(initialStock + 5);
    }

    [Fact]
    public void IncreaseStock_ShouldRaiseProductStockUpdatedEvent()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        product.ClearDomainEvents();

        // Act
        product.IncreaseStock(5, "Test increase");

        // Assert
        var domainEvent = product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductStockUpdatedEvent>().Subject;
        
        domainEvent.ProductId.Should().Be(product.Id);
        domainEvent.PreviousStock.Should().Be(10);
        domainEvent.NewStock.Should().Be(15);
        domainEvent.Reason.Should().Be("Test increase");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void IncreaseStock_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => product.IncreaseStock(invalidQuantity));
        exception.Message.Should().Contain("Quantity must be positive");
    }

    [Fact]
    public void DecreaseStock_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act
        product.DecreaseStock(3);

        // Assert
        product.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void DecreaseStock_WithQuantityExceedingStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act & Assert
        var exception = Assert.Throws<InsufficientStockException>(() => product.DecreaseStock(15));
        exception.Message.Should().Contain("Insufficient stock. Requested: 15, Available: 10");
    }

    [Fact]
    public void SetStock_WithValidQuantity_ShouldSetStock()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act
        product.SetStock(20);

        // Assert
        product.StockQuantity.Should().Be(20);
    }

    [Fact]
    public void UpdateMinimumStockLevel_WithValidLevel_ShouldUpdateLevel()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act
        product.UpdateMinimumStockLevel(8);

        // Assert
        product.MinimumStockLevel.Should().Be(8);
    }

    [Fact]
    public void AddReview_WithValidParameters_ShouldAddReview()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var customerId = Guid.NewGuid();

        // Act
        product.AddReview(customerId, 5, "Great product", "Really love this product!");

        // Assert
        product.Reviews.Should().HaveCount(1);
        var review = product.Reviews.First();
        review.CustomerId.Should().Be(customerId);
        review.Rating.Should().Be(5);
        review.Title.Should().Be("Great product");
        review.Content.Should().Be("Really love this product!");
    }

    [Fact]
    public void AddReview_ShouldRaiseProductReviewAddedEvent()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        product.ClearDomainEvents();
        var customerId = Guid.NewGuid();

        // Act
        product.AddReview(customerId, 5, "Great product", "Really love this product!");

        // Assert
        var domainEvent = product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductReviewAddedEvent>().Subject;
        
        domainEvent.ProductId.Should().Be(product.Id);
        domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.Rating.Should().Be(5);
    }

    [Fact]
    public void AddReview_WhenCustomerAlreadyReviewed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var customerId = Guid.NewGuid();
        product.AddReview(customerId, 5, "Great product", "Really love this product!");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            product.AddReview(customerId, 4, "Another review", "Different content"));
        
        exception.Message.Should().Contain("Customer has already reviewed this product");
    }

    [Fact]
    public void RemoveReview_WithExistingReview_ShouldRemoveReview()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var customerId = Guid.NewGuid();
        product.AddReview(customerId, 5, "Great product", "Really love this product!");
        var reviewId = product.Reviews.First().Id;

        // Act
        product.RemoveReview(reviewId);

        // Assert
        product.Reviews.Should().BeEmpty();
    }

    [Fact]
    public void GetReview_WithExistingReview_ShouldReturnReview()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var customerId = Guid.NewGuid();
        product.AddReview(customerId, 5, "Great product", "Really love this product!");
        var reviewId = product.Reviews.First().Id;

        // Act
        var review = product.GetReview(reviewId);

        // Assert
        review.Should().NotBeNull();
        review!.Id.Should().Be(reviewId);
    }

    [Fact]
    public void GetReview_WithNonExistingReview_ShouldReturnNull()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var nonExistingId = Guid.NewGuid();

        // Act
        var review = product.GetReview(nonExistingId);

        // Assert
        review.Should().BeNull();
    }

    [Fact]
    public void CanFulfillOrder_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act
        var canFulfill = product.CanFulfillOrder(5);

        // Assert
        canFulfill.Should().BeTrue();
    }

    [Fact]
    public void CanFulfillOrder_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act
        var canFulfill = product.CanFulfillOrder(15);

        // Assert
        canFulfill.Should().BeFalse();
    }

    [Fact]
    public void CanFulfillOrder_WithInactiveProduct_ShouldReturnFalse()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        product.Deactivate();

        // Act
        var canFulfill = product.CanFulfillOrder(5);

        // Assert
        canFulfill.Should().BeFalse();
    }

    [Fact]
    public void CanFulfillOrder_WithOutOfStock_ShouldReturnFalse()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 0, 5, _categoryId);

        // Act
        var canFulfill = product.CanFulfillOrder(1);

        // Assert
        canFulfill.Should().BeFalse();
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(0, false)]
    public void IsInStock_ShouldReturnCorrectValue(int stockQuantity, bool expectedResult)
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", stockQuantity, 5, _categoryId);

        // Act & Assert
        product.IsInStock.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(10, 5, false)] // Stock above minimum
    [InlineData(5, 5, true)]   // Stock at minimum
    [InlineData(3, 5, true)]   // Stock below minimum but not zero
    [InlineData(0, 5, false)]  // Out of stock
    public void IsLowStock_ShouldReturnCorrectValue(int stockQuantity, int minimumLevel, bool expectedResult)
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", stockQuantity, minimumLevel, _categoryId);

        // Act & Assert
        product.IsLowStock.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(10, false)]
    public void IsOutOfStock_ShouldReturnCorrectValue(int stockQuantity, bool expectedResult)
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", stockQuantity, 5, _categoryId);

        // Act & Assert
        product.IsOutOfStock.Should().Be(expectedResult);
    }

    [Fact]
    public void AverageRating_WithNoApprovedReviews_ShouldReturnZero()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);

        // Act & Assert
        product.AverageRating.Should().Be(0);
    }

    [Fact]
    public void AverageRating_WithApprovedReviews_ShouldReturnCorrectAverage()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var customer1 = Guid.NewGuid();
        var customer2 = Guid.NewGuid();
        
        product.AddReview(customer1, 5, "Great", "Excellent product");
        product.AddReview(customer2, 3, "Good", "Good product");
        
        // Approve the reviews
        var review1 = product.Reviews.First(r => r.CustomerId == customer1);
        var review2 = product.Reviews.First(r => r.CustomerId == customer2);
        review1.Approve();
        review2.Approve();

        // Act & Assert
        product.AverageRating.Should().Be(4m); // (5 + 3) / 2 = 4
    }

    [Fact]
    public void ReviewCount_WithApprovedReviews_ShouldReturnCorrectCount()
    {
        // Arrange
        var product = Product.Create("Test", "Description", _validPrice, "TEST-001", 10, 5, _categoryId);
        var customer1 = Guid.NewGuid();
        var customer2 = Guid.NewGuid();
        
        product.AddReview(customer1, 5, "Great", "Excellent product");
        product.AddReview(customer2, 3, "Good", "Good product");
        
        // Approve only one review
        var review1 = product.Reviews.First(r => r.CustomerId == customer1);
        review1.Approve();

        // Act & Assert
        product.ReviewCount.Should().Be(1);
    }
}
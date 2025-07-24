using ECommerce.Application.Commands.Products;
using ECommerce.Application.Validators.Products;

namespace ECommerce.Application.Tests.Validators.Products;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _validator = new CreateProductCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid(),
            Weight: 1.5m,
            Dimensions: "10x10x10"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyName_ShouldFail(string name)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: name,
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_NameTooLong_ShouldFail()
    {
        // Arrange
        var longName = new string('A', 256); // 256 characters
        var command = new CreateProductCommand(
            Name: longName,
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name) && 
                                           e.ErrorMessage.Contains("cannot exceed 255 characters"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyDescription_ShouldFail(string description)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: description,
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Description));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public void Validate_InvalidPrice_ShouldFail(decimal price)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: price,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Price));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Validate_InvalidCurrency_ShouldFail(string currency)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: currency,
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Currency));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptySku_ShouldFail(string sku)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: sku,
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Sku));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_NegativeStockQuantity_ShouldFail(int stockQuantity)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: stockQuantity,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.StockQuantity));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_NegativeMinimumStockLevel_ShouldFail(int minimumStockLevel)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: minimumStockLevel,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MinimumStockLevel));
    }

    [Fact]
    public void Validate_EmptyCategoryId_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.Empty
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CategoryId));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.5)]
    public void Validate_NegativeWeight_ShouldFail(decimal weight)
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            Currency: "USD",
            Sku: "TEST-001",
            StockQuantity: 10,
            MinimumStockLevel: 5,
            CategoryId: Guid.NewGuid(),
            Weight: weight
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Weight));
    }
}
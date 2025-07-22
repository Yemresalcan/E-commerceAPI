using ECommerce.Domain.Events;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Aggregates.ProductAggregate;

/// <summary>
/// Product aggregate root that manages product lifecycle, inventory, and reviews
/// </summary>
public class Product : AggregateRoot
{
    private readonly List<ProductReview> _reviews = [];

    /// <summary>
    /// The name of the product
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The description of the product
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// The price of the product
    /// </summary>
    public Money Price { get; private set; }

    /// <summary>
    /// The SKU (Stock Keeping Unit) of the product
    /// </summary>
    public string Sku { get; private set; }

    /// <summary>
    /// The current stock quantity
    /// </summary>
    public int StockQuantity { get; private set; }

    /// <summary>
    /// The minimum stock level before reordering
    /// </summary>
    public int MinimumStockLevel { get; private set; }

    /// <summary>
    /// The category this product belongs to
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Whether the product is currently active/available
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Whether the product is featured
    /// </summary>
    public bool IsFeatured { get; private set; }

    /// <summary>
    /// The weight of the product in grams
    /// </summary>
    public decimal Weight { get; private set; }

    /// <summary>
    /// Product dimensions in centimeters (Length x Width x Height)
    /// </summary>
    public string Dimensions { get; private set; }

    /// <summary>
    /// Read-only collection of product reviews
    /// </summary>
    public IReadOnlyCollection<ProductReview> Reviews => _reviews.AsReadOnly();

    /// <summary>
    /// Average rating calculated from approved reviews
    /// </summary>
    public decimal AverageRating => CalculateAverageRating();

    /// <summary>
    /// Total number of approved reviews
    /// </summary>
    public int ReviewCount => _reviews.Count(r => r.IsApproved);

    /// <summary>
    /// Whether the product is in stock
    /// </summary>
    public bool IsInStock => StockQuantity > 0;

    /// <summary>
    /// Whether the product stock is below minimum level
    /// </summary>
    public bool IsLowStock => StockQuantity <= MinimumStockLevel && StockQuantity > 0;

    /// <summary>
    /// Whether the product is out of stock
    /// </summary>
    public bool IsOutOfStock => StockQuantity <= 0;

    // Private constructor for EF Core
    private Product() : base()
    {
        Name = string.Empty;
        Description = string.Empty;
        Price = Money.Zero("USD");
        Sku = string.Empty;
        Dimensions = string.Empty;
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    public static Product Create(
        string name,
        string description,
        Money price,
        string sku,
        int stockQuantity,
        int minimumStockLevel,
        Guid categoryId,
        decimal weight = 0,
        string dimensions = "")
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidatePrice(price);
        ValidateSku(sku);
        ValidateStockQuantity(stockQuantity);
        ValidateMinimumStockLevel(minimumStockLevel);
        ValidateWeight(weight);

        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Sku = sku,
            StockQuantity = stockQuantity,
            MinimumStockLevel = minimumStockLevel,
            CategoryId = categoryId,
            IsActive = true,
            IsFeatured = false,
            Weight = weight,
            Dimensions = dimensions ?? string.Empty
        };

        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Price.Amount,
            product.Price.Currency,
            product.CategoryId,
            product.StockQuantity));

        return product;
    }

    /// <summary>
    /// Updates product information
    /// </summary>
    public void Update(string name, string description, Money price, decimal weight = 0, string dimensions = "")
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidatePrice(price);
        ValidateWeight(weight);

        Name = name;
        Description = description;
        Price = price;
        Weight = weight;
        Dimensions = dimensions ?? string.Empty;

        MarkAsModified();

        AddDomainEvent(new ProductUpdatedEvent(
            Id,
            Name,
            Price.Amount,
            Price.Currency));
    }

    /// <summary>
    /// Updates the product category
    /// </summary>
    public void UpdateCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));
        }

        CategoryId = categoryId;
        MarkAsModified();
    }

    /// <summary>
    /// Activates the product
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsModified();
    }

    /// <summary>
    /// Deactivates the product
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsModified();
    }

    /// <summary>
    /// Marks the product as featured
    /// </summary>
    public void MarkAsFeatured()
    {
        IsFeatured = true;
        MarkAsModified();
    }

    /// <summary>
    /// Removes featured status from the product
    /// </summary>
    public void RemoveFeaturedStatus()
    {
        IsFeatured = false;
        MarkAsModified();
    }

    /// <summary>
    /// Increases stock quantity
    /// </summary>
    public void IncreaseStock(int quantity, string reason = "Stock replenishment")
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        var previousStock = StockQuantity;
        StockQuantity += quantity;
        MarkAsModified();

        AddDomainEvent(new ProductStockUpdatedEvent(
            Id,
            previousStock,
            StockQuantity,
            reason));
    }

    /// <summary>
    /// Decreases stock quantity
    /// </summary>
    public void DecreaseStock(int quantity, string reason = "Stock reduction")
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        if (quantity > StockQuantity)
        {
            throw new InsufficientStockException(quantity, StockQuantity);
        }

        var previousStock = StockQuantity;
        StockQuantity -= quantity;
        MarkAsModified();

        AddDomainEvent(new ProductStockUpdatedEvent(
            Id,
            previousStock,
            StockQuantity,
            reason));
    }

    /// <summary>
    /// Sets the stock quantity to a specific value
    /// </summary>
    public void SetStock(int quantity, string reason = "Stock adjustment")
    {
        ValidateStockQuantity(quantity);

        var previousStock = StockQuantity;
        StockQuantity = quantity;
        MarkAsModified();

        AddDomainEvent(new ProductStockUpdatedEvent(
            Id,
            previousStock,
            StockQuantity,
            reason));
    }

    /// <summary>
    /// Updates the minimum stock level
    /// </summary>
    public void UpdateMinimumStockLevel(int minimumStockLevel)
    {
        ValidateMinimumStockLevel(minimumStockLevel);

        MinimumStockLevel = minimumStockLevel;
        MarkAsModified();
    }

    /// <summary>
    /// Adds a review to the product
    /// </summary>
    public void AddReview(Guid customerId, int rating, string title, string content, bool isVerified = false)
    {
        // Check if customer has already reviewed this product
        if (_reviews.Any(r => r.CustomerId == customerId))
        {
            throw new InvalidOperationException("Customer has already reviewed this product");
        }

        var review = ProductReview.Create(Id, customerId, rating, title, content, isVerified);
        _reviews.Add(review);
        MarkAsModified();

        AddDomainEvent(new ProductReviewAddedEvent(
            Id,
            review.Id,
            customerId,
            rating,
            isVerified));
    }

    /// <summary>
    /// Removes a review from the product
    /// </summary>
    public void RemoveReview(Guid reviewId)
    {
        var review = _reviews.FirstOrDefault(r => r.Id == reviewId);
        if (review != null)
        {
            _reviews.Remove(review);
            MarkAsModified();
        }
    }

    /// <summary>
    /// Gets a specific review by ID
    /// </summary>
    public ProductReview? GetReview(Guid reviewId)
    {
        return _reviews.FirstOrDefault(r => r.Id == reviewId);
    }

    /// <summary>
    /// Checks if the product can fulfill a specific quantity order
    /// </summary>
    public bool CanFulfillOrder(int requestedQuantity)
    {
        return IsActive && IsInStock && StockQuantity >= requestedQuantity;
    }

    private decimal CalculateAverageRating()
    {
        var approvedReviews = _reviews.Where(r => r.IsApproved).ToList();
        if (!approvedReviews.Any())
        {
            return 0;
        }

        return (decimal)approvedReviews.Average(r => r.Rating);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("Product name cannot exceed 255 characters", nameof(name));
        }
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Product description cannot be null or empty", nameof(description));
        }

        if (description.Length > 2000)
        {
            throw new ArgumentException("Product description cannot exceed 2000 characters", nameof(description));
        }
    }

    private static void ValidatePrice(Money price)
    {
        ArgumentNullException.ThrowIfNull(price);

        if (!price.IsPositive)
        {
            throw new InvalidProductPriceException(price.Amount);
        }
    }

    private static void ValidateSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Product SKU cannot be null or empty", nameof(sku));
        }

        if (sku.Length > 50)
        {
            throw new ArgumentException("Product SKU cannot exceed 50 characters", nameof(sku));
        }
    }

    private static void ValidateStockQuantity(int stockQuantity)
    {
        if (stockQuantity < 0)
        {
            throw new InvalidStockQuantityException(stockQuantity);
        }
    }

    private static void ValidateMinimumStockLevel(int minimumStockLevel)
    {
        if (minimumStockLevel < 0)
        {
            throw new ArgumentException("Minimum stock level cannot be negative", nameof(minimumStockLevel));
        }
    }

    private static void ValidateWeight(decimal weight)
    {
        if (weight < 0)
        {
            throw new ArgumentException("Product weight cannot be negative", nameof(weight));
        }
    }
}
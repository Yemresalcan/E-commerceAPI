namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a product with invalid stock quantity
/// </summary>
public class InvalidStockQuantityException : DomainException
{
    public InvalidStockQuantityException(int stockQuantity) 
        : base($"Stock quantity cannot be negative. Provided: {stockQuantity}")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a product with invalid price
/// </summary>
public class InvalidProductPriceException : DomainException
{
    public InvalidProductPriceException(decimal price) 
        : base($"Product price must be positive. Provided: {price}")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to reduce stock below zero
/// </summary>
public class InsufficientStockException : DomainException
{
    public InsufficientStockException(int requested, int available) 
        : base($"Insufficient stock. Requested: {requested}, Available: {available}")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a product review with invalid rating
/// </summary>
public class InvalidRatingException : DomainException
{
    public InvalidRatingException(int rating) 
        : base($"Rating must be between 1 and 5. Provided: {rating}")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a product review without content
/// </summary>
public class InvalidReviewContentException : DomainException
{
    public InvalidReviewContentException() 
        : base("Review content cannot be null or empty")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a category with invalid parent relationship
/// </summary>
public class InvalidCategoryHierarchyException : DomainException
{
    public InvalidCategoryHierarchyException(string message) 
        : base(message)
    {
    }
}
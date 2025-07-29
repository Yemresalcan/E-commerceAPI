using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Tests.Aggregates;

public class ProductReviewTests
{
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_ShouldCreateReview()
    {
        // Arrange
        var rating = 5;
        var title = "Great product";
        var content = "This is an excellent product that I highly recommend.";
        var isVerified = true;

        // Act
        var review = ProductReview.Create(_productId, _customerId, rating, title, content, isVerified);

        // Assert
        review.Should().NotBeNull();
        review.ProductId.Should().Be(_productId);
        review.CustomerId.Should().Be(_customerId);
        review.Rating.Should().Be(rating);
        review.Title.Should().Be(title);
        review.Content.Should().Be(content);
        review.IsVerified.Should().Be(isVerified);
        review.IsApproved.Should().BeFalse(); // Should be false by default
        review.HelpfulVotes.Should().Be(0);
    }

    [Fact]
    public void Create_WithoutVerification_ShouldCreateUnverifiedReview()
    {
        // Act
        var review = ProductReview.Create(_productId, _customerId, 4, "Good", "Nice product");

        // Assert
        review.IsVerified.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Create_WithInvalidRating_ShouldThrowInvalidRatingException(int invalidRating)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidRatingException>(() =>
            ProductReview.Create(_productId, _customerId, invalidRating, "Title", "Content"));
        
        exception.Message.Should().Contain($"Rating must be between 1 and 5. Provided: {invalidRating}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_WithValidRating_ShouldCreateReview(int validRating)
    {
        // Act
        var review = ProductReview.Create(_productId, _customerId, validRating, "Title", "Content");

        // Assert
        review.Rating.Should().Be(validRating);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ProductReview.Create(_productId, _customerId, 5, invalidTitle, "Content"));
        
        exception.Message.Should().Contain("Review title cannot be null or empty");
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longTitle = new string('a', 201);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ProductReview.Create(_productId, _customerId, 5, longTitle, "Content"));
        
        exception.Message.Should().Contain("Review title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidContent_ShouldThrowInvalidReviewContentException(string invalidContent)
    {
        // Act & Assert
        Assert.Throws<InvalidReviewContentException>(() =>
            ProductReview.Create(_productId, _customerId, 5, "Title", invalidContent));
    }

    [Fact]
    public void Create_WithContentTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longContent = new string('a', 2001);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ProductReview.Create(_productId, _customerId, 5, "Title", longContent));
        
        exception.Message.Should().Contain("Review content cannot exceed 2000 characters");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateReview()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 3, "Original Title", "Original content");
        review.Approve(); // Approve first
        
        var newRating = 5;
        var newTitle = "Updated Title";
        var newContent = "Updated content with more details";

        // Act
        review.Update(newRating, newTitle, newContent);

        // Assert
        review.Rating.Should().Be(newRating);
        review.Title.Should().Be(newTitle);
        review.Content.Should().Be(newContent);
        review.IsApproved.Should().BeFalse(); // Should reset approval status
    }

    [Fact]
    public void Update_WithInvalidRating_ShouldThrowInvalidRatingException()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 3, "Title", "Content");

        // Act & Assert
        Assert.Throws<InvalidRatingException>(() =>
            review.Update(0, "New Title", "New content"));
    }

    [Fact]
    public void Update_WithInvalidTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 3, "Title", "Content");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            review.Update(4, "", "New content"));
        
        exception.Message.Should().Contain("Review title cannot be null or empty");
    }

    [Fact]
    public void Update_WithInvalidContent_ShouldThrowInvalidReviewContentException()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 3, "Title", "Content");

        // Act & Assert
        Assert.Throws<InvalidReviewContentException>(() =>
            review.Update(4, "New Title", ""));
    }

    [Fact]
    public void Approve_ShouldSetIsApprovedToTrue()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");

        // Act
        review.Approve();

        // Assert
        review.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Reject_ShouldSetIsApprovedToFalse()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");
        review.Approve(); // First approve it

        // Act
        review.Reject();

        // Assert
        review.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void MarkAsVerified_ShouldSetIsVerifiedToTrue()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");

        // Act
        review.MarkAsVerified();

        // Assert
        review.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void AddHelpfulVote_ShouldIncreaseHelpfulVotes()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");
        var initialVotes = review.HelpfulVotes;

        // Act
        review.AddHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(initialVotes + 1);
    }

    [Fact]
    public void AddHelpfulVote_MultipleTimes_ShouldIncreaseCorrectly()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");

        // Act
        review.AddHelpfulVote();
        review.AddHelpfulVote();
        review.AddHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(3);
    }

    [Fact]
    public void RemoveHelpfulVote_WithExistingVotes_ShouldDecreaseHelpfulVotes()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");
        review.AddHelpfulVote();
        review.AddHelpfulVote();
        var votesBeforeRemoval = review.HelpfulVotes;

        // Act
        review.RemoveHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(votesBeforeRemoval - 1);
    }

    [Fact]
    public void RemoveHelpfulVote_WithZeroVotes_ShouldNotGoNegative()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");

        // Act
        review.RemoveHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(0);
    }

    [Fact]
    public void RemoveHelpfulVote_WithOneVote_ShouldBecomeZero()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");
        review.AddHelpfulVote();

        // Act
        review.RemoveHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(0);
    }

    [Fact]
    public void Create_WithMaximumValidTitle_ShouldCreateReview()
    {
        // Arrange
        var maxTitle = new string('a', 200);

        // Act
        var review = ProductReview.Create(_productId, _customerId, 5, maxTitle, "Content");

        // Assert
        review.Title.Should().Be(maxTitle);
    }

    [Fact]
    public void Create_WithMaximumValidContent_ShouldCreateReview()
    {
        // Arrange
        var maxContent = new string('a', 2000);

        // Act
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", maxContent);

        // Assert
        review.Content.Should().Be(maxContent);
    }

    [Fact]
    public void ReviewWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var review = ProductReview.Create(_productId, _customerId, 4, "Good product", "I like this product");

        // Act & Assert - Initial state
        review.IsApproved.Should().BeFalse();
        review.IsVerified.Should().BeFalse();
        review.HelpfulVotes.Should().Be(0);

        // Act & Assert - Mark as verified
        review.MarkAsVerified();
        review.IsVerified.Should().BeTrue();

        // Act & Assert - Approve review
        review.Approve();
        review.IsApproved.Should().BeTrue();

        // Act & Assert - Add helpful votes
        review.AddHelpfulVote();
        review.AddHelpfulVote();
        review.HelpfulVotes.Should().Be(2);

        // Act & Assert - Update review (should reset approval)
        review.Update(5, "Excellent product", "Updated review content");
        review.IsApproved.Should().BeFalse();
        review.Rating.Should().Be(5);
        review.Title.Should().Be("Excellent product");
        review.Content.Should().Be("Updated review content");
        review.IsVerified.Should().BeTrue(); // Should remain verified
        review.HelpfulVotes.Should().Be(2); // Should remain the same
    }
}
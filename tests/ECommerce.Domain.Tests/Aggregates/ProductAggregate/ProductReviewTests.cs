using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ECommerce.Domain.Tests.Aggregates.ProductAggregate;

public class ProductReviewTests
{
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_ShouldCreateReview()
    {
        // Arrange
        var rating = 5;
        var title = "Great product!";
        var content = "I really love this product. It works perfectly.";
        var isVerified = true;

        // Act
        var review = ProductReview.Create(_productId, _customerId, rating, title, content, isVerified);

        // Assert
        review.Should().NotBeNull();
        review.Id.Should().NotBe(Guid.Empty);
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
    public void Create_WithDefaultVerification_ShouldCreateUnverifiedReview()
    {
        // Act
        var review = ProductReview.Create(_productId, _customerId, 5, "Title", "Content");

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
        var act = () => ProductReview.Create(_productId, _customerId, invalidRating, "Title", "Content");
        act.Should().Throw<InvalidRatingException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Act & Assert
        var act = () => ProductReview.Create(_productId, _customerId, 5, invalidTitle, "Content");
        act.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var longTitle = new string('a', 201);

        // Act & Assert
        var act = () => ProductReview.Create(_productId, _customerId, 5, longTitle, "Content");
        act.Should().Throw<ArgumentException>().WithMessage("*title*exceed*200*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidContent_ShouldThrowInvalidReviewContentException(string invalidContent)
    {
        // Act & Assert
        var act = () => ProductReview.Create(_productId, _customerId, 5, "Title", invalidContent);
        act.Should().Throw<InvalidReviewContentException>();
    }

    [Fact]
    public void Create_WithTooLongContent_ShouldThrowArgumentException()
    {
        // Arrange
        var longContent = new string('a', 2001);

        // Act & Assert
        var act = () => ProductReview.Create(_productId, _customerId, 5, "Title", longContent);
        act.Should().Throw<ArgumentException>().WithMessage("*content*exceed*2000*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateReview()
    {
        // Arrange
        var review = CreateValidReview();
        var newRating = 4;
        var newTitle = "Updated title";
        var newContent = "Updated content with more details.";

        // Act
        review.Update(newRating, newTitle, newContent);

        // Assert
        review.Rating.Should().Be(newRating);
        review.Title.Should().Be(newTitle);
        review.Content.Should().Be(newContent);
        review.IsApproved.Should().BeFalse(); // Should reset approval status
    }

    [Fact]
    public void Update_ShouldResetApprovalStatus()
    {
        // Arrange
        var review = CreateValidReview();
        review.Approve();

        // Act
        review.Update(4, "New title", "New content");

        // Assert
        review.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldSetIsApprovedToTrue()
    {
        // Arrange
        var review = CreateValidReview();

        // Act
        review.Approve();

        // Assert
        review.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Reject_ShouldSetIsApprovedToFalse()
    {
        // Arrange
        var review = CreateValidReview();
        review.Approve();

        // Act
        review.Reject();

        // Assert
        review.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void MarkAsVerified_ShouldSetIsVerifiedToTrue()
    {
        // Arrange
        var review = CreateValidReview();

        // Act
        review.MarkAsVerified();

        // Assert
        review.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void AddHelpfulVote_ShouldIncreaseHelpfulVotes()
    {
        // Arrange
        var review = CreateValidReview();
        var initialVotes = review.HelpfulVotes;

        // Act
        review.AddHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(initialVotes + 1);
    }

    [Fact]
    public void RemoveHelpfulVote_WithExistingVotes_ShouldDecreaseHelpfulVotes()
    {
        // Arrange
        var review = CreateValidReview();
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
        var review = CreateValidReview();

        // Act
        review.RemoveHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(0);
    }

    [Fact]
    public void AddHelpfulVote_MultipleTimes_ShouldIncreaseCorrectly()
    {
        // Arrange
        var review = CreateValidReview();

        // Act
        review.AddHelpfulVote();
        review.AddHelpfulVote();
        review.AddHelpfulVote();

        // Assert
        review.HelpfulVotes.Should().Be(3);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_WithValidRatings_ShouldCreateReview(int validRating)
    {
        // Act
        var review = ProductReview.Create(_productId, _customerId, validRating, "Title", "Content");

        // Assert
        review.Rating.Should().Be(validRating);
    }

    [Fact]
    public void Update_WithInvalidRating_ShouldThrowInvalidRatingException()
    {
        // Arrange
        var review = CreateValidReview();

        // Act & Assert
        var act = () => review.Update(0, "Title", "Content");
        act.Should().Throw<InvalidRatingException>();
    }

    [Fact]
    public void Update_WithInvalidTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var review = CreateValidReview();

        // Act & Assert
        var act = () => review.Update(5, "", "Content");
        act.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Fact]
    public void Update_WithInvalidContent_ShouldThrowInvalidReviewContentException()
    {
        // Arrange
        var review = CreateValidReview();

        // Act & Assert
        var act = () => review.Update(5, "Title", "");
        act.Should().Throw<InvalidReviewContentException>();
    }

    private ProductReview CreateValidReview()
    {
        return ProductReview.Create(
            _productId,
            _customerId,
            5,
            "Great product!",
            "I really love this product. It works perfectly.");
    }
}
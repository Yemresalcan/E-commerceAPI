using ECommerce.Domain.Aggregates.ProductAggregate;
using ECommerce.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ECommerce.Domain.Tests.Aggregates.ProductAggregate;

public class CategoryTests
{
    [Fact]
    public void CreateRoot_WithValidParameters_ShouldCreateRootCategory()
    {
        // Arrange
        var name = "Electronics";
        var description = "Electronic products and accessories";

        // Act
        var category = Category.CreateRoot(name, description);

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().NotBe(Guid.Empty);
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.ParentCategoryId.Should().BeNull();
        category.Level.Should().Be(0);
        category.IsActive.Should().BeTrue();
        category.IsRoot.Should().BeTrue();
        category.HasChildren.Should().BeFalse();
        category.Children.Should().BeEmpty();
    }

    [Fact]
    public void CreateChild_WithValidParameters_ShouldCreateChildCategory()
    {
        // Arrange
        var name = "Smartphones";
        var description = "Mobile phones and smartphones";
        var parentId = Guid.NewGuid();
        var parentLevel = 0;

        // Act
        var category = Category.CreateChild(name, description, parentId, parentLevel);

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.ParentCategoryId.Should().Be(parentId);
        category.Level.Should().Be(parentLevel + 1);
        category.IsActive.Should().BeTrue();
        category.IsRoot.Should().BeFalse();
        category.HasChildren.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateRoot_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        var act = () => Category.CreateRoot(invalidName, "Valid description");
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void CreateRoot_WithTooLongName_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act & Assert
        var act = () => Category.CreateRoot(longName, "Valid description");
        act.Should().Throw<ArgumentException>().WithMessage("*name*exceed*100*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateRoot_WithInvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Act & Assert
        var act = () => Category.CreateRoot("Valid name", invalidDescription);
        act.Should().Throw<ArgumentException>().WithMessage("*description*");
    }

    [Fact]
    public void CreateRoot_WithTooLongDescription_ShouldThrowArgumentException()
    {
        // Arrange
        var longDescription = new string('a', 501);

        // Act & Assert
        var act = () => Category.CreateRoot("Valid name", longDescription);
        act.Should().Throw<ArgumentException>().WithMessage("*description*exceed*500*");
    }

    [Fact]
    public void CreateChild_WithMaxHierarchyLevel_ShouldThrowInvalidCategoryHierarchyException()
    {
        // Arrange
        var parentLevel = 5; // Max level

        // Act & Assert
        var act = () => Category.CreateChild("Name", "Description", Guid.NewGuid(), parentLevel);
        act.Should().Throw<InvalidCategoryHierarchyException>().WithMessage("*hierarchy cannot exceed*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCategory()
    {
        // Arrange
        var category = Category.CreateRoot("Original Name", "Original Description");
        var newName = "Updated Name";
        var newDescription = "Updated Description";

        // Act
        category.Update(newName, newDescription);

        // Assert
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var category = Category.CreateRoot("Name", "Description");
        category.Deactivate();

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WithoutChildren_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = Category.CreateRoot("Name", "Description");

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WithChildren_ShouldThrowInvalidCategoryHierarchyException()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");
        var childCategory = Category.CreateChild("Child", "Child Description", parentCategory.Id, parentCategory.Level);
        parentCategory.AddChild(childCategory);

        // Act & Assert
        var act = () => parentCategory.Deactivate();
        act.Should().Throw<InvalidCategoryHierarchyException>().WithMessage("*active child categories*");
    }

    [Fact]
    public void AddChild_WithValidChild_ShouldAddChild()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");
        var childCategory = Category.CreateChild("Child", "Child Description", parentCategory.Id, parentCategory.Level);

        // Act
        parentCategory.AddChild(childCategory);

        // Assert
        parentCategory.HasChildren.Should().BeTrue();
        parentCategory.Children.Should().ContainSingle().Which.Should().Be(childCategory);
    }

    [Fact]
    public void AddChild_WithMismatchedParentId_ShouldThrowInvalidCategoryHierarchyException()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");
        var childCategory = Category.CreateChild("Child", "Child Description", Guid.NewGuid(), parentCategory.Level);

        // Act & Assert
        var act = () => parentCategory.AddChild(childCategory);
        act.Should().Throw<InvalidCategoryHierarchyException>().WithMessage("*parent ID must match*");
    }

    [Fact]
    public void AddChild_WithDuplicateChild_ShouldThrowInvalidCategoryHierarchyException()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");
        var childCategory = Category.CreateChild("Child", "Child Description", parentCategory.Id, parentCategory.Level);
        parentCategory.AddChild(childCategory);

        // Act & Assert
        var act = () => parentCategory.AddChild(childCategory);
        act.Should().Throw<InvalidCategoryHierarchyException>().WithMessage("*already exists*");
    }

    [Fact]
    public void RemoveChild_WithExistingChild_ShouldRemoveChild()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");
        var childCategory = Category.CreateChild("Child", "Child Description", parentCategory.Id, parentCategory.Level);
        parentCategory.AddChild(childCategory);

        // Act
        parentCategory.RemoveChild(childCategory);

        // Assert
        parentCategory.HasChildren.Should().BeFalse();
        parentCategory.Children.Should().BeEmpty();
    }

    [Fact]
    public void RemoveChild_WithNonExistingChild_ShouldNotThrow()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");
        var childCategory = Category.CreateChild("Child", "Child Description", parentCategory.Id, parentCategory.Level);

        // Act & Assert
        var act = () => parentCategory.RemoveChild(childCategory);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddChild_WithNullChild_ShouldThrowArgumentNullException()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");

        // Act & Assert
        var act = () => parentCategory.AddChild(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveChild_WithNullChild_ShouldThrowArgumentNullException()
    {
        // Arrange
        var parentCategory = Category.CreateRoot("Parent", "Parent Description");

        // Act & Assert
        var act = () => parentCategory.RemoveChild(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
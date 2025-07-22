using ECommerce.Domain.Aggregates;

namespace ECommerce.Domain.Tests.Aggregates;

public class EntityTests
{
    private class TestEntity : Entity
    {
        public string Name { get; private set; } = string.Empty;

        public TestEntity() : base() { }
        public TestEntity(Guid id) : base(id) { }

        public void SetName(string name)
        {
            Name = name;
            MarkAsModified();
        }
    }

    [Fact]
    public void Entity_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Entity_Should_Initialize_With_Specific_Id()
    {
        // Arrange
        var specificId = Guid.NewGuid();

        // Act
        var entity = new TestEntity(specificId);

        // Assert
        entity.Id.Should().Be(specificId);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsModified_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;

        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        entity.SetName("Test Name");

        // Assert
        entity.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void CreatedAt_Should_Not_Change_After_MarkAsModified()
    {
        // Arrange
        var entity = new TestEntity();
        var originalCreatedAt = entity.CreatedAt;

        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        entity.SetName("Test Name");

        // Assert
        entity.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public void Equals_Should_Return_True_For_Same_Entity_Reference()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.Equals(entity).Should().BeTrue();
        (entity == entity).Should().BeTrue();
        (entity != entity).Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_True_For_Entities_With_Same_Id()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_Return_False_For_Entities_With_Different_Ids()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_When_Compared_To_Null()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
        (entity == null).Should().BeFalse();
        (entity != null).Should().BeTrue();
        (null == entity).Should().BeFalse();
        (null != entity).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_Return_False_When_Compared_To_Different_Type()
    {
        // Arrange
        var entity = new TestEntity();
        var differentObject = "not an entity";

        // Act & Assert
        entity.Equals(differentObject).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Should_Return_Same_Value_For_Same_Id()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_Return_Different_Values_For_Different_Ids()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act & Assert
        entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_Be_Based_On_Id()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.GetHashCode().Should().Be(entity.Id.GetHashCode());
    }
}
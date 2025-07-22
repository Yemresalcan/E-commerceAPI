using ECommerce.Domain.Aggregates;
using ECommerce.Domain.Events;

namespace ECommerce.Domain.Tests.Aggregates;

public class AggregateRootTests
{
    private class TestAggregate : AggregateRoot
    {
        public TestAggregate() : base() { }
        public TestAggregate(Guid id) : base(id) { }

        public void AddTestEvent(DomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }

        public void RemoveTestEvent(DomainEvent domainEvent)
        {
            RemoveDomainEvent(domainEvent);
        }

        public void MarkTestAsModified()
        {
            MarkAsModified();
        }
    }

    private record TestDomainEvent(string TestProperty) : DomainEvent;

    [Fact]
    public void AggregateRoot_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var aggregate = new TestAggregate();

        // Assert
        aggregate.Id.Should().NotBe(Guid.Empty);
        aggregate.Version.Should().Be(1);
        aggregate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        aggregate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AggregateRoot_Should_Initialize_With_Specific_Id()
    {
        // Arrange
        var specificId = Guid.NewGuid();

        // Act
        var aggregate = new TestAggregate(specificId);

        // Assert
        aggregate.Id.Should().Be(specificId);
        aggregate.Version.Should().Be(1);
        aggregate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        aggregate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_Should_Add_Event_To_Collection()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var domainEvent = new TestDomainEvent("test");

        // Act
        aggregate.AddTestEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_Should_Add_Multiple_Events_To_Collection()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var event1 = new TestDomainEvent("test1");
        var event2 = new TestDomainEvent("test2");

        // Act
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(2);
        aggregate.DomainEvents.Should().Contain(event1);
        aggregate.DomainEvents.Should().Contain(event2);
    }

    [Fact]
    public void AddDomainEvent_Should_Throw_ArgumentNullException_When_Event_Is_Null()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act & Assert
        var action = () => aggregate.AddTestEvent(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveDomainEvent_Should_Remove_Event_From_Collection()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var event1 = new TestDomainEvent("test1");
        var event2 = new TestDomainEvent("test2");
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);

        // Act
        aggregate.RemoveTestEvent(event1);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().NotContain(event1);
        aggregate.DomainEvents.Should().Contain(event2);
    }

    [Fact]
    public void RemoveDomainEvent_Should_Throw_ArgumentNullException_When_Event_Is_Null()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act & Assert
        var action = () => aggregate.RemoveTestEvent(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All_Events_From_Collection()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var event1 = new TestDomainEvent("test1");
        var event2 = new TestDomainEvent("test2");
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_Should_Return_ReadOnly_Collection()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var domainEvent = new TestDomainEvent("test");
        aggregate.AddTestEvent(domainEvent);

        // Act
        var domainEvents = aggregate.DomainEvents;

        // Assert
        domainEvents.Should().BeAssignableTo<IReadOnlyCollection<DomainEvent>>();
        domainEvents.Should().HaveCount(1);
        domainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void MarkAsModified_Should_Update_UpdatedAt_And_Increment_Version()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var originalUpdatedAt = aggregate.UpdatedAt;
        var originalVersion = aggregate.Version;

        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        aggregate.MarkTestAsModified();

        // Assert
        aggregate.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        aggregate.Version.Should().Be(originalVersion + 1);
    }

    [Fact]
    public void MarkAsModified_Should_Increment_Version_Multiple_Times()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var originalVersion = aggregate.Version;

        // Act
        aggregate.MarkTestAsModified();
        aggregate.MarkTestAsModified();
        aggregate.MarkTestAsModified();

        // Assert
        aggregate.Version.Should().Be(originalVersion + 3);
    }

    [Fact]
    public void CreatedAt_Should_Not_Change_After_MarkAsModified()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var originalCreatedAt = aggregate.CreatedAt;

        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        aggregate.MarkTestAsModified();

        // Assert
        aggregate.CreatedAt.Should().Be(originalCreatedAt);
    }
}
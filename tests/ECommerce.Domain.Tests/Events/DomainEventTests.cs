using ECommerce.Domain.Events;

namespace ECommerce.Domain.Tests.Events;

public class DomainEventTests
{
    private record TestDomainEvent(string TestProperty) : DomainEvent;

    [Fact]
    public void DomainEvent_Should_Have_Unique_Id()
    {
        // Arrange & Act
        var event1 = new TestDomainEvent("test1");
        var event2 = new TestDomainEvent("test2");

        // Assert
        event1.Id.Should().NotBe(Guid.Empty);
        event2.Id.Should().NotBe(Guid.Empty);
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void DomainEvent_Should_Have_OccurredOn_Set_To_Current_Time()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new TestDomainEvent("test");
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredOn.Should().BeOnOrBefore(afterCreation);
        domainEvent.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void DomainEvent_Should_Have_Default_Version_Of_One()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent("test");

        // Assert
        domainEvent.Version.Should().Be(1);
    }

    [Fact]
    public void DomainEvent_Records_Should_Support_Equality_Comparison()
    {
        // Arrange
        var event1 = new TestDomainEvent("test");
        var event2 = new TestDomainEvent("test");

        // Act & Assert
        event1.Should().NotBe(event2); // Different instances with different IDs
        event1.TestProperty.Should().Be(event2.TestProperty); // Same property values
    }
}
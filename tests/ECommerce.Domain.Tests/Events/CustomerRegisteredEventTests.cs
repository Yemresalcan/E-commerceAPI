using ECommerce.Domain.Events;

namespace ECommerce.Domain.Tests.Events;

public class CustomerRegisteredEventTests
{
    [Fact]
    public void CustomerRegisteredEvent_Should_Initialize_All_Properties_Correctly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var phoneNumber = "+1-555-123-4567";

        // Act
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            email,
            firstName,
            lastName,
            phoneNumber);

        // Assert
        customerRegisteredEvent.CustomerId.Should().Be(customerId);
        customerRegisteredEvent.Email.Should().Be(email);
        customerRegisteredEvent.FirstName.Should().Be(firstName);
        customerRegisteredEvent.LastName.Should().Be(lastName);
        customerRegisteredEvent.PhoneNumber.Should().Be(phoneNumber);
        customerRegisteredEvent.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customerRegisteredEvent.Id.Should().NotBe(Guid.Empty);
        customerRegisteredEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customerRegisteredEvent.Version.Should().Be(1);
    }

    [Fact]
    public void CustomerRegisteredEvent_Should_Initialize_Without_Phone_Number()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            email,
            firstName,
            lastName);

        // Assert
        customerRegisteredEvent.CustomerId.Should().Be(customerId);
        customerRegisteredEvent.Email.Should().Be(email);
        customerRegisteredEvent.FirstName.Should().Be(firstName);
        customerRegisteredEvent.LastName.Should().Be(lastName);
        customerRegisteredEvent.PhoneNumber.Should().BeNull();
        customerRegisteredEvent.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CustomerRegisteredEvent_Should_Inherit_From_DomainEvent()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            "test@example.com",
            "John",
            "Doe");

        // Assert
        customerRegisteredEvent.Should().BeAssignableTo<DomainEvent>();
    }

    [Fact]
    public void CustomerRegisteredEvent_Should_Support_Record_Equality()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var event1 = new CustomerRegisteredEvent(
            customerId,
            "test@example.com",
            "John",
            "Doe",
            "+1-555-123-4567");

        var event2 = new CustomerRegisteredEvent(
            customerId,
            "test@example.com",
            "John",
            "Doe",
            "+1-555-123-4567");

        // Act & Assert
        event1.Should().NotBe(event2); // Different instances have different IDs and timestamps
        event1.CustomerId.Should().Be(event2.CustomerId);
        event1.Email.Should().Be(event2.Email);
        event1.FirstName.Should().Be(event2.FirstName);
        event1.LastName.Should().Be(event2.LastName);
        event1.PhoneNumber.Should().Be(event2.PhoneNumber);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("very.long.email.address@subdomain.example.com")]
    public void CustomerRegisteredEvent_Should_Accept_Various_Email_Formats(string email)
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            email,
            "John",
            "Doe");

        // Assert
        customerRegisteredEvent.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("John", "Doe")]
    [InlineData("Mary-Jane", "Smith-Johnson")]
    [InlineData("José", "García")]
    [InlineData("", "")]
    public void CustomerRegisteredEvent_Should_Accept_Various_Name_Formats(string firstName, string lastName)
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            "test@example.com",
            firstName,
            lastName);

        // Assert
        customerRegisteredEvent.FirstName.Should().Be(firstName);
        customerRegisteredEvent.LastName.Should().Be(lastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("+1-555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("555.123.4567")]
    public void CustomerRegisteredEvent_Should_Accept_Various_Phone_Number_Formats(string? phoneNumber)
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            "test@example.com",
            "John",
            "Doe",
            phoneNumber);

        // Assert
        customerRegisteredEvent.PhoneNumber.Should().Be(phoneNumber);
    }

    [Fact]
    public void CustomerRegisteredEvent_RegistrationDate_Should_Be_In_UTC()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var customerRegisteredEvent = new CustomerRegisteredEvent(
            customerId,
            "test@example.com",
            "John",
            "Doe");

        // Assert
        customerRegisteredEvent.RegistrationDate.Kind.Should().Be(DateTimeKind.Utc);
    }
}
using ECommerce.Domain.Aggregates.CustomerAggregate;
using ECommerce.Domain.Events;
using ECommerce.Domain.ValueObjects;
using FluentAssertions;

namespace ECommerce.Domain.Tests.Aggregates.CustomerAggregate;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = new Email("john.doe@example.com");
        var phoneNumber = new PhoneNumber("+1234567890");

        // Act
        var customer = Customer.Create(firstName, lastName, email, phoneNumber);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().NotBeEmpty();
        customer.FirstName.Should().Be(firstName);
        customer.LastName.Should().Be(lastName);
        customer.Email.Should().Be(email);
        customer.PhoneNumber.Should().Be(phoneNumber);
        customer.IsActive.Should().BeTrue();
        customer.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.LastActiveDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.Profile.Should().NotBeNull();
        customer.Addresses.Should().BeEmpty();
        customer.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Create_WithoutPhoneNumber_ShouldCreateCustomerSuccessfully()
    {
        // Arrange
        var firstName = "Jane";
        var lastName = "Smith";
        var email = new Email("jane.smith@example.com");

        // Act
        var customer = Customer.Create(firstName, lastName, email);

        // Assert
        customer.Should().NotBeNull();
        customer.FirstName.Should().Be(firstName);
        customer.LastName.Should().Be(lastName);
        customer.Email.Should().Be(email);
        customer.PhoneNumber.Should().BeNull();
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRaiseCustomerRegisteredEvent()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = new Email("john.doe@example.com");
        var phoneNumber = new PhoneNumber("+1234567890");

        // Act
        var customer = Customer.Create(firstName, lastName, email, phoneNumber);

        // Assert
        customer.DomainEvents.Should().HaveCount(1);
        var domainEvent = customer.DomainEvents.First();
        domainEvent.Should().BeOfType<CustomerRegisteredEvent>();
        
        var customerRegisteredEvent = (CustomerRegisteredEvent)domainEvent;
        customerRegisteredEvent.CustomerId.Should().Be(customer.Id);
        customerRegisteredEvent.Email.Should().Be(email.Value);
        customerRegisteredEvent.FirstName.Should().Be(firstName);
        customerRegisteredEvent.LastName.Should().Be(lastName);
        customerRegisteredEvent.PhoneNumber.Should().Be(phoneNumber.Value);
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void Create_WithInvalidNames_ShouldThrowArgumentException(string firstName, string lastName)
    {
        // Arrange
        var email = new Email("john.doe@example.com");

        // Act & Assert
        var action = () => Customer.Create(firstName, lastName, email);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";

        // Act & Assert
        var action = () => Customer.Create(firstName, lastName, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("ThisIsAVeryLongFirstNameThatExceedsFiftyCharactersLimit")]
    [InlineData("John", "ThisIsAVeryLongLastNameThatExceedsFiftyCharactersLimit")]
    public void Create_WithNamesTooLong_ShouldThrowArgumentException(string firstName, string lastName = "Doe")
    {
        // Arrange
        var email = new Email("john.doe@example.com");
        if (lastName == "Doe" && firstName.Length > 50)
            lastName = "Doe";

        // Act & Assert
        var action = () => Customer.Create(firstName, lastName, email);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newPhoneNumber = new PhoneNumber("+1987654321");
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.UpdateBasicInfo(newFirstName, newLastName, newPhoneNumber);

        // Assert
        customer.FirstName.Should().Be(newFirstName);
        customer.LastName.Should().Be(newLastName);
        customer.PhoneNumber.Should().Be(newPhoneNumber);
        customer.FullName.Should().Be("Jane Smith");
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateSuccessfully()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var newEmail = new Email("john.smith@example.com");
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.UpdateEmail(newEmail);

        // Assert
        customer.Email.Should().Be(newEmail);
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithSameEmail_ShouldNotUpdate()
    {
        // Arrange
        var email = new Email("john.doe@example.com");
        var customer = Customer.Create("John", "Doe", email);
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.UpdateEmail(email);

        // Assert
        customer.Email.Should().Be(email);
        customer.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));

        // Act & Assert
        var action = () => customer.UpdateEmail(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAddress_WithValidAddress_ShouldAddSuccessfully()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var address = Address.Create(
            AddressType.Shipping,
            "123 Main St",
            "Anytown",
            "CA",
            "12345",
            "USA");

        // Act
        customer.AddAddress(address);

        // Assert
        customer.Addresses.Should().HaveCount(1);
        customer.Addresses.First().Should().Be(address);
        customer.HasAddresses.Should().BeTrue();
        customer.HasPrimaryAddress.Should().BeTrue();
        customer.PrimaryAddress.Should().Be(address);
        address.IsPrimary.Should().BeTrue(); // First address should be set as primary
    }

    [Fact]
    public void AddAddress_MultipleAddresses_ShouldMaintainOnePrimary()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var address1 = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        var address2 = Address.Create(AddressType.Billing, "456 Oak Ave", "Otherville", "NY", "67890", "USA", isPrimary: true);

        // Act
        customer.AddAddress(address1);
        customer.AddAddress(address2);

        // Assert
        customer.Addresses.Should().HaveCount(2);
        customer.Addresses.Count(a => a.IsPrimary).Should().Be(1);
        customer.PrimaryAddress.Should().Be(address2);
        address1.IsPrimary.Should().BeFalse();
        address2.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void RemoveAddress_WithExistingAddress_ShouldRemoveSuccessfully()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        customer.AddAddress(address);

        // Act
        customer.RemoveAddress(address.Id);

        // Assert
        customer.Addresses.Should().BeEmpty();
        customer.HasAddresses.Should().BeFalse();
        customer.HasPrimaryAddress.Should().BeFalse();
        customer.PrimaryAddress.Should().BeNull();
    }

    [Fact]
    public void RemoveAddress_WithNonExistentAddress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var action = () => customer.RemoveAddress(nonExistentId);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"Address with ID {nonExistentId} not found.");
    }

    [Fact]
    public void RemoveAddress_RemovingPrimaryAddress_ShouldSetNewPrimary()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var address1 = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        var address2 = Address.Create(AddressType.Billing, "456 Oak Ave", "Otherville", "NY", "67890", "USA");
        customer.AddAddress(address1);
        customer.AddAddress(address2);

        // Act
        customer.RemoveAddress(address1.Id); // Remove primary address

        // Assert
        customer.Addresses.Should().HaveCount(1);
        customer.PrimaryAddress.Should().Be(address2);
        address2.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimaryAddress_WithExistingAddress_ShouldSetAsPrimary()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var address1 = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        var address2 = Address.Create(AddressType.Billing, "456 Oak Ave", "Otherville", "NY", "67890", "USA");
        customer.AddAddress(address1);
        customer.AddAddress(address2);

        // Act
        customer.SetPrimaryAddress(address2.Id);

        // Assert
        customer.PrimaryAddress.Should().Be(address2);
        address1.IsPrimary.Should().BeFalse();
        address2.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimaryAddress_WithNonExistentAddress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var action = () => customer.SetPrimaryAddress(nonExistentId);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"Address with ID {nonExistentId} not found.");
    }

    [Fact]
    public void UpdateProfile_WithValidProfile_ShouldUpdateSuccessfully()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var newProfile = Profile.Create("es", PreferredCurrency.EUR, "Europe/Madrid");
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.UpdateProfile(newProfile);

        // Assert
        customer.Profile.Should().Be(newProfile);
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateProfile_WithNullProfile_ShouldThrowArgumentNullException()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));

        // Act & Assert
        var action = () => customer.UpdateProfile(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateCustomer()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotUpdate()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        customer.Deactivate();
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
        customer.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Reactivate_WhenInactive_ShouldReactivateCustomer()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        customer.Deactivate();
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.Reactivate();

        // Assert
        customer.IsActive.Should().BeTrue();
        customer.LastActiveDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Reactivate_WhenAlreadyActive_ShouldNotUpdate()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var originalUpdatedAt = customer.UpdatedAt;
        var originalLastActiveDate = customer.LastActiveDate;

        // Act
        customer.Reactivate();

        // Assert
        customer.IsActive.Should().BeTrue();
        customer.LastActiveDate.Should().Be(originalLastActiveDate);
        customer.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void RecordActivity_ShouldUpdateLastActiveDate()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var originalLastActiveDate = customer.LastActiveDate;
        var originalUpdatedAt = customer.UpdatedAt;

        // Act
        customer.RecordActivity();

        // Assert
        customer.LastActiveDate.Should().BeAfter(originalLastActiveDate!.Value);
        customer.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void GetAddressesByType_ShouldReturnCorrectAddresses()
    {
        // Arrange
        var customer = Customer.Create("John", "Doe", new Email("john.doe@example.com"));
        var shippingAddress = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        var billingAddress = Address.Create(AddressType.Billing, "456 Oak Ave", "Otherville", "NY", "67890", "USA");
        var bothAddress = Address.Create(AddressType.Both, "789 Pine Rd", "Somewhere", "TX", "54321", "USA");
        
        customer.AddAddress(shippingAddress);
        customer.AddAddress(billingAddress);
        customer.AddAddress(bothAddress);

        // Act
        var shippingAddresses = customer.GetAddressesByType(AddressType.Shipping);
        var billingAddresses = customer.GetAddressesByType(AddressType.Billing);
        var bothAddresses = customer.GetAddressesByType(AddressType.Both);

        // Assert
        shippingAddresses.Should().HaveCount(1).And.Contain(shippingAddress);
        billingAddresses.Should().HaveCount(1).And.Contain(billingAddress);
        bothAddresses.Should().HaveCount(1).And.Contain(bothAddress);
    }
}
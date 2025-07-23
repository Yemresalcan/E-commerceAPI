using ECommerce.Domain.Aggregates.CustomerAggregate;
using FluentAssertions;

namespace ECommerce.Domain.Tests.Aggregates.CustomerAggregate;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAddressSuccessfully()
    {
        // Arrange
        var type = AddressType.Shipping;
        var street1 = "123 Main Street";
        var city = "Anytown";
        var state = "California";
        var postalCode = "12345";
        var country = "USA";
        var street2 = "Apt 4B";
        var label = "Home";

        // Act
        var address = Address.Create(type, street1, city, state, postalCode, country, street2, label, true);

        // Assert
        address.Should().NotBeNull();
        address.Id.Should().NotBeEmpty();
        address.Type.Should().Be(type);
        address.Street1.Should().Be(street1);
        address.Street2.Should().Be(street2);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.PostalCode.Should().Be(postalCode);
        address.Country.Should().Be(country);
        address.Label.Should().Be(label);
        address.IsPrimary.Should().BeTrue();
        address.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        address.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithMinimalData_ShouldCreateAddressSuccessfully()
    {
        // Arrange
        var type = AddressType.Billing;
        var street1 = "456 Oak Avenue";
        var city = "Otherville";
        var state = "New York";
        var postalCode = "67890";
        var country = "USA";

        // Act
        var address = Address.Create(type, street1, city, state, postalCode, country);

        // Assert
        address.Should().NotBeNull();
        address.Type.Should().Be(type);
        address.Street1.Should().Be(street1);
        address.Street2.Should().BeNull();
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.PostalCode.Should().Be(postalCode);
        address.Country.Should().Be(country);
        address.Label.Should().BeNull();
        address.IsPrimary.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "City", "State", "12345", "Country")]
    [InlineData("   ", "City", "State", "12345", "Country")]
    [InlineData("Street", "", "State", "12345", "Country")]
    [InlineData("Street", "   ", "State", "12345", "Country")]
    [InlineData("Street", "City", "", "12345", "Country")]
    [InlineData("Street", "City", "   ", "12345", "Country")]
    [InlineData("Street", "City", "State", "", "Country")]
    [InlineData("Street", "City", "State", "   ", "Country")]
    [InlineData("Street", "City", "State", "12345", "")]
    [InlineData("Street", "City", "State", "12345", "   ")]
    public void Create_WithInvalidRequiredFields_ShouldThrowArgumentException(
        string street1, string city, string state, string postalCode, string country)
    {
        // Act & Assert
        var action = () => Address.Create(AddressType.Shipping, street1, city, state, postalCode, country);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("This is a very long street address that exceeds the maximum allowed length of 100 characters for street1")]
    public void Create_WithStreet1TooLong_ShouldThrowArgumentException(string street1)
    {
        // Act & Assert
        var action = () => Address.Create(AddressType.Shipping, street1, "City", "State", "12345", "Country");
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Street address line 1 cannot exceed 100 characters*");
    }

    [Theory]
    [InlineData("This is a very long city name that exceeds fifty chars")]
    public void Create_WithCityTooLong_ShouldThrowArgumentException(string city)
    {
        // Act & Assert
        var action = () => Address.Create(AddressType.Shipping, "Street", city, "State", "12345", "Country");
        action.Should().Throw<ArgumentException>()
            .WithMessage("*City cannot exceed 50 characters*");
    }

    [Theory]
    [InlineData("This is a very long state name that exceeds fifty chars")]
    public void Create_WithStateTooLong_ShouldThrowArgumentException(string state)
    {
        // Act & Assert
        var action = () => Address.Create(AddressType.Shipping, "Street", "City", state, "12345", "Country");
        action.Should().Throw<ArgumentException>()
            .WithMessage("*State cannot exceed 50 characters*");
    }

    [Theory]
    [InlineData("This is a very long postal code that exceeds twenty characters")]
    public void Create_WithPostalCodeTooLong_ShouldThrowArgumentException(string postalCode)
    {
        // Act & Assert
        var action = () => Address.Create(AddressType.Shipping, "Street", "City", "State", postalCode, "Country");
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Postal code cannot exceed 20 characters*");
    }

    [Theory]
    [InlineData("This is a very long country name that exceeds fifty chars")]
    public void Create_WithCountryTooLong_ShouldThrowArgumentException(string country)
    {
        // Act & Assert
        var action = () => Address.Create(AddressType.Shipping, "Street", "City", "State", "12345", country);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Country cannot exceed 50 characters*");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        var originalUpdatedAt = address.UpdatedAt;
        var newType = AddressType.Billing;
        var newStreet1 = "456 Oak Ave";
        var newCity = "Otherville";
        var newState = "NY";
        var newPostalCode = "67890";
        var newCountry = "USA";
        var newStreet2 = "Suite 100";
        var newLabel = "Office";

        // Act
        address.Update(newType, newStreet1, newCity, newState, newPostalCode, newCountry, newStreet2, newLabel);

        // Assert
        address.Type.Should().Be(newType);
        address.Street1.Should().Be(newStreet1);
        address.Street2.Should().Be(newStreet2);
        address.City.Should().Be(newCity);
        address.State.Should().Be(newState);
        address.PostalCode.Should().Be(newPostalCode);
        address.Country.Should().Be(newCountry);
        address.Label.Should().Be(newLabel);
        address.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateLabel_WithValidLabel_ShouldUpdateSuccessfully()
    {
        // Arrange
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        var originalUpdatedAt = address.UpdatedAt;
        var newLabel = "Home Address";

        // Act
        address.UpdateLabel(newLabel);

        // Assert
        address.Label.Should().Be(newLabel);
        address.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateLabel_WithNullOrEmptyLabel_ShouldSetToNull()
    {
        // Arrange
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA", label: "Original");

        // Act
        address.UpdateLabel(null);

        // Assert
        address.Label.Should().BeNull();

        // Act
        address.UpdateLabel("");

        // Assert
        address.Label.Should().BeNull();

        // Act
        address.UpdateLabel("   ");

        // Assert
        address.Label.Should().BeNull();
    }

    [Fact]
    public void UpdateLabel_WithLabelTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");
        var longLabel = new string('A', 51);

        // Act & Assert
        var action = () => address.UpdateLabel(longLabel);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Address label cannot exceed 50 characters*");
    }

    [Fact]
    public void FullAddress_ShouldReturnFormattedMultiLineAddress()
    {
        // Arrange
        var address = Address.Create(
            AddressType.Shipping,
            "123 Main Street",
            "Anytown",
            "California",
            "12345",
            "USA",
            "Apt 4B");

        // Act
        var fullAddress = address.FullAddress;

        // Assert
        var expectedAddress = $"123 Main Street{Environment.NewLine}Apt 4B{Environment.NewLine}Anytown, California 12345{Environment.NewLine}USA";
        fullAddress.Should().Be(expectedAddress);
    }

    [Fact]
    public void FullAddress_WithoutStreet2_ShouldReturnFormattedMultiLineAddress()
    {
        // Arrange
        var address = Address.Create(
            AddressType.Shipping,
            "123 Main Street",
            "Anytown",
            "California",
            "12345",
            "USA");

        // Act
        var fullAddress = address.FullAddress;

        // Assert
        var expectedAddress = $"123 Main Street{Environment.NewLine}Anytown, California 12345{Environment.NewLine}USA";
        fullAddress.Should().Be(expectedAddress);
    }

    [Fact]
    public void SingleLineAddress_ShouldReturnFormattedSingleLineAddress()
    {
        // Arrange
        var address = Address.Create(
            AddressType.Shipping,
            "123 Main Street",
            "Anytown",
            "California",
            "12345",
            "USA",
            "Apt 4B");

        // Act
        var singleLineAddress = address.SingleLineAddress;

        // Assert
        singleLineAddress.Should().Be("123 Main Street, Apt 4B, Anytown, California 12345, USA");
    }

    [Theory]
    [InlineData(AddressType.Shipping, true, false)]
    [InlineData(AddressType.Billing, false, true)]
    [InlineData(AddressType.Both, true, true)]
    public void AddressTypeProperties_ShouldReturnCorrectValues(AddressType type, bool canShip, bool canBill)
    {
        // Arrange
        var address = Address.Create(type, "123 Main St", "Anytown", "CA", "12345", "USA");

        // Act & Assert
        address.CanBeUsedForShipping.Should().Be(canShip);
        address.CanBeUsedForBilling.Should().Be(canBill);
    }

    [Theory]
    [InlineData("USA", "usa", true)]
    [InlineData("USA", "US", false)]
    [InlineData("Canada", "CANADA", true)]
    [InlineData("Canada", "ca", false)]
    public void IsInCountry_ShouldReturnCorrectResult(string addressCountry, string checkCountry, bool expected)
    {
        // Arrange
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", addressCountry);

        // Act
        var result = address.IsInCountry(checkCountry);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("US", "12345", true)]
    [InlineData("US", "12345-6789", true)]
    [InlineData("US", "1234", false)]
    [InlineData("US", "123456", false)]
    [InlineData("CA", "K1A 0A6", true)]
    [InlineData("CA", "K1A0A6", true)]
    [InlineData("CA", "12345", false)]
    [InlineData("UK", "SW1A 1AA", true)]
    [InlineData("UK", "M1 1AA", true)]
    [InlineData("UK", "12345", false)]
    [InlineData("DE", "12345", true)] // Generic validation for other countries
    [InlineData("DE", "12", false)] // Too short
    [InlineData("DE", "12345678901", false)] // Too long
    public void HasValidPostalCodeFormat_ShouldReturnCorrectResult(string country, string postalCode, bool expected)
    {
        // Arrange
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "State", postalCode, country);

        // Act
        var result = address.HasValidPostalCodeFormat();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Create_WithPrimaryFlag_ShouldSetPrimaryCorrectly()
    {
        // Arrange & Act
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA", isPrimary: true);

        // Assert
        address.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutPrimaryFlag_ShouldNotBePrimary()
    {
        // Arrange & Act
        var address = Address.Create(AddressType.Shipping, "123 Main St", "Anytown", "CA", "12345", "USA");

        // Assert
        address.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldTrimWhitespaceFromAllFields()
    {
        // Arrange
        var type = AddressType.Shipping;
        var street1 = "  123 Main Street  ";
        var street2 = "  Apt 4B  ";
        var city = "  Anytown  ";
        var state = "  California  ";
        var postalCode = "  12345  ";
        var country = "  USA  ";
        var label = "  Home  ";

        // Act
        var address = Address.Create(type, street1, city, state, postalCode, country, street2, label);

        // Assert
        address.Street1.Should().Be("123 Main Street");
        address.Street2.Should().Be("Apt 4B");
        address.City.Should().Be("Anytown");
        address.State.Should().Be("California");
        address.PostalCode.Should().Be("12345");
        address.Country.Should().Be("USA");
        address.Label.Should().Be("Home");
    }
}
using ECommerce.Domain.Aggregates.CustomerAggregate;
using FluentAssertions;

namespace ECommerce.Domain.Tests.Aggregates.CustomerAggregate;

public class ProfileTests
{
    [Fact]
    public void CreateDefault_ShouldCreateProfileWithDefaultValues()
    {
        // Act
        var profile = Profile.CreateDefault();

        // Assert
        profile.Should().NotBeNull();
        profile.Id.Should().NotBeEmpty();
        profile.PreferredLanguage.Should().Be("en");
        profile.PreferredCurrency.Should().Be(PreferredCurrency.USD);
        profile.Timezone.Should().Be("UTC");
        profile.CommunicationPreference.Should().Be(CommunicationPreference.Email);
        profile.ReceiveMarketingEmails.Should().BeTrue();
        profile.ReceiveOrderNotifications.Should().BeTrue();
        profile.ReceivePromotionalSms.Should().BeFalse();
        profile.LoyaltyTier.Should().Be("Bronze");
        profile.LoyaltyPoints.Should().Be(0);
        profile.IsVerified.Should().BeFalse();
        profile.DateOfBirth.Should().BeNull();
        profile.Gender.Should().BeNull();
        profile.Interests.Should().BeNull();
        profile.VerificationDate.Should().BeNull();
        profile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        profile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithCustomValues_ShouldCreateProfileSuccessfully()
    {
        // Arrange
        var language = "es";
        var currency = PreferredCurrency.EUR;
        var timezone = "Europe/Madrid";
        var communication = CommunicationPreference.SMS;

        // Act
        var profile = Profile.Create(language, currency, timezone, communication);

        // Assert
        profile.PreferredLanguage.Should().Be(language);
        profile.PreferredCurrency.Should().Be(currency);
        profile.Timezone.Should().Be(timezone);
        profile.CommunicationPreference.Should().Be(communication);
        profile.ReceiveMarketingEmails.Should().BeTrue();
        profile.ReceiveOrderNotifications.Should().BeTrue();
        profile.ReceivePromotionalSms.Should().BeFalse();
        profile.LoyaltyTier.Should().Be("Bronze");
        profile.LoyaltyPoints.Should().Be(0);
        profile.IsVerified.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "UTC")]
    [InlineData("   ", "UTC")]
    [InlineData("en", "")]
    [InlineData("en", "   ")]
    public void Create_WithInvalidLanguageOrTimezone_ShouldThrowArgumentException(string language, string timezone)
    {
        // Act & Assert
        var action = () => Profile.Create(language, PreferredCurrency.USD, timezone);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("eng")] // Too long
    [InlineData("e")] // Too short
    public void Create_WithInvalidLanguageLength_ShouldThrowArgumentException(string language)
    {
        // Act & Assert
        var action = () => Profile.Create(language, PreferredCurrency.USD, "UTC");
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Preferred language must be a 2-character ISO 639-1 code*");
    }

    [Fact]
    public void Create_WithLongTimezone_ShouldThrowArgumentException()
    {
        // Arrange
        var longTimezone = new string('A', 51);

        // Act & Assert
        var action = () => Profile.Create("en", PreferredCurrency.USD, longTimezone);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Timezone cannot exceed 50 characters*");
    }

    [Fact]
    public void UpdateLocalizationPreferences_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var originalUpdatedAt = profile.UpdatedAt;
        var newLanguage = "fr";
        var newCurrency = PreferredCurrency.EUR;
        var newTimezone = "Europe/Paris";

        // Act
        profile.UpdateLocalizationPreferences(newLanguage, newCurrency, newTimezone);

        // Assert
        profile.PreferredLanguage.Should().Be(newLanguage);
        profile.PreferredCurrency.Should().Be(newCurrency);
        profile.Timezone.Should().Be(newTimezone);
        profile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateCommunicationPreferences_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var originalUpdatedAt = profile.UpdatedAt;
        var newCommunication = CommunicationPreference.Phone;
        var receiveMarketing = false;
        var receiveOrders = true;
        var receiveSms = true;

        // Act
        profile.UpdateCommunicationPreferences(newCommunication, receiveMarketing, receiveOrders, receiveSms);

        // Assert
        profile.CommunicationPreference.Should().Be(newCommunication);
        profile.ReceiveMarketingEmails.Should().Be(receiveMarketing);
        profile.ReceiveOrderNotifications.Should().Be(receiveOrders);
        profile.ReceivePromotionalSms.Should().Be(receiveSms);
        profile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdatePersonalInfo_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var originalUpdatedAt = profile.UpdatedAt;
        var dateOfBirth = new DateTime(1990, 5, 15);
        var gender = "Male";
        var interests = "Technology, Sports, Music";

        // Act
        profile.UpdatePersonalInfo(dateOfBirth, gender, interests);

        // Assert
        profile.DateOfBirth.Should().Be(dateOfBirth);
        profile.Gender.Should().Be(gender);
        profile.Interests.Should().Be(interests);
        profile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdatePersonalInfo_WithFutureDateOfBirth_ShouldThrowArgumentException()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var futureDate = DateTime.Today.AddDays(1);

        // Act & Assert
        var action = () => profile.UpdatePersonalInfo(futureDate, null, null);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Date of birth cannot be in the future*");
    }

    [Fact]
    public void UpdatePersonalInfo_WithVeryOldDateOfBirth_ShouldThrowArgumentException()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var veryOldDate = DateTime.Today.AddYears(-151);

        // Act & Assert
        var action = () => profile.UpdatePersonalInfo(veryOldDate, null, null);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Date of birth cannot be more than 150 years ago*");
    }

    [Fact]
    public void UpdatePersonalInfo_WithLongGender_ShouldThrowArgumentException()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var longGender = new string('A', 21);

        // Act & Assert
        var action = () => profile.UpdatePersonalInfo(null, longGender, null);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Gender cannot exceed 20 characters*");
    }

    [Fact]
    public void UpdatePersonalInfo_WithLongInterests_ShouldThrowArgumentException()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var longInterests = new string('A', 501);

        // Act & Assert
        var action = () => profile.UpdatePersonalInfo(null, null, longInterests);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Interests cannot exceed 500 characters*");
    }

    [Fact]
    public void Age_WithDateOfBirth_ShouldCalculateCorrectAge()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var dateOfBirth = DateTime.Today.AddYears(-25).AddDays(-1); // Ensure birthday has passed

        // Act
        profile.UpdatePersonalInfo(dateOfBirth, null, null);

        // Assert
        profile.Age.Should().Be(25);
    }

    [Fact]
    public void Age_WithoutDateOfBirth_ShouldReturnNull()
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act & Assert
        profile.Age.Should().BeNull();
    }

    [Fact]
    public void IsAdult_WithAgeOver18_ShouldReturnTrue()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var dateOfBirth = DateTime.Today.AddYears(-25);

        // Act
        profile.UpdatePersonalInfo(dateOfBirth, null, null);

        // Assert
        profile.IsAdult.Should().BeTrue();
    }

    [Fact]
    public void IsAdult_WithAgeUnder18_ShouldReturnFalse()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var dateOfBirth = DateTime.Today.AddYears(-16);

        // Act
        profile.UpdatePersonalInfo(dateOfBirth, null, null);

        // Assert
        profile.IsAdult.Should().BeFalse();
    }

    [Fact]
    public void IsAdult_WithoutDateOfBirth_ShouldReturnFalse()
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act & Assert
        profile.IsAdult.Should().BeFalse();
    }

    [Theory]
    [InlineData(PreferredCurrency.USD, "$")]
    [InlineData(PreferredCurrency.EUR, "€")]
    [InlineData(PreferredCurrency.GBP, "£")]
    [InlineData(PreferredCurrency.CAD, "C$")]
    [InlineData(PreferredCurrency.AUD, "A$")]
    [InlineData(PreferredCurrency.JPY, "¥")]
    public void CurrencySymbol_ShouldReturnCorrectSymbol(PreferredCurrency currency, string expectedSymbol)
    {
        // Arrange
        var profile = Profile.Create("en", currency);

        // Act & Assert
        profile.CurrencySymbol.Should().Be(expectedSymbol);
    }

    [Fact]
    public void AddLoyaltyPoints_WithValidPoints_ShouldAddSuccessfully()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var originalUpdatedAt = profile.UpdatedAt;
        var pointsToAdd = 500;

        // Act
        profile.AddLoyaltyPoints(pointsToAdd);

        // Assert
        profile.LoyaltyPoints.Should().Be(pointsToAdd);
        profile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void AddLoyaltyPoints_WithNegativePoints_ShouldThrowArgumentException()
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act & Assert
        var action = () => profile.AddLoyaltyPoints(-100);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Points to add cannot be negative*");
    }

    [Fact]
    public void RedeemLoyaltyPoints_WithValidPoints_ShouldRedeemSuccessfully()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        profile.AddLoyaltyPoints(1000);
        var originalUpdatedAt = profile.UpdatedAt;
        var pointsToRedeem = 300;

        // Act
        profile.RedeemLoyaltyPoints(pointsToRedeem);

        // Assert
        profile.LoyaltyPoints.Should().Be(700);
        profile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void RedeemLoyaltyPoints_WithMorePointsThanAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        profile.AddLoyaltyPoints(100);

        // Act & Assert
        var action = () => profile.RedeemLoyaltyPoints(200);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot redeem more points than available*");
    }

    [Fact]
    public void RedeemLoyaltyPoints_WithNegativePoints_ShouldThrowArgumentException()
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act & Assert
        var action = () => profile.RedeemLoyaltyPoints(-100);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Points to redeem cannot be negative*");
    }

    [Theory]
    [InlineData(0, "Bronze")]
    [InlineData(500, "Bronze")]
    [InlineData(1000, "Silver")]
    [InlineData(2500, "Silver")]
    [InlineData(5000, "Gold")]
    [InlineData(7500, "Gold")]
    [InlineData(10000, "Platinum")]
    [InlineData(15000, "Platinum")]
    public void LoyaltyTier_ShouldUpdateBasedOnPoints(int points, string expectedTier)
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act
        profile.AddLoyaltyPoints(points);

        // Assert
        profile.LoyaltyTier.Should().Be(expectedTier);
    }

    [Fact]
    public void MarkAsVerified_WhenNotVerified_ShouldMarkAsVerified()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var originalUpdatedAt = profile.UpdatedAt;

        // Act
        profile.MarkAsVerified();

        // Assert
        profile.IsVerified.Should().BeTrue();
        profile.VerificationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        profile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsVerified_WhenAlreadyVerified_ShouldNotUpdate()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        profile.MarkAsVerified();
        var originalUpdatedAt = profile.UpdatedAt;
        var originalVerificationDate = profile.VerificationDate;

        // Act
        profile.MarkAsVerified();

        // Assert
        profile.IsVerified.Should().BeTrue();
        profile.VerificationDate.Should().Be(originalVerificationDate);
        profile.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsUnverified_WhenVerified_ShouldMarkAsUnverified()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        profile.MarkAsVerified();
        var originalUpdatedAt = profile.UpdatedAt;

        // Act
        profile.MarkAsUnverified();

        // Assert
        profile.IsVerified.Should().BeFalse();
        profile.VerificationDate.Should().BeNull();
        profile.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsUnverified_WhenNotVerified_ShouldNotUpdate()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var originalUpdatedAt = profile.UpdatedAt;

        // Act
        profile.MarkAsUnverified();

        // Assert
        profile.IsVerified.Should().BeFalse();
        profile.VerificationDate.Should().BeNull();
        profile.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Theory]
    [InlineData(CommunicationPreference.Email, true, true, false, true)]
    [InlineData(CommunicationPreference.Email, false, true, false, true)]
    [InlineData(CommunicationPreference.Email, false, false, false, false)]
    [InlineData(CommunicationPreference.SMS, true, true, true, true)]
    [InlineData(CommunicationPreference.SMS, true, true, false, false)]
    [InlineData(CommunicationPreference.Phone, true, true, false, true)]
    [InlineData(CommunicationPreference.None, true, true, true, false)]
    public void CanReceiveCommunication_ShouldReturnCorrectResult(
        CommunicationPreference method,
        bool receiveMarketing,
        bool receiveOrders,
        bool receiveSms,
        bool expected)
    {
        // Arrange
        var profile = Profile.CreateDefault();
        profile.UpdateCommunicationPreferences(CommunicationPreference.Phone, receiveMarketing, receiveOrders, receiveSms);

        // Act
        var result = profile.CanReceiveCommunication(method);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetMinimumAgeRequirement_ShouldReturn13()
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act
        var minAge = profile.GetMinimumAgeRequirement();

        // Assert
        minAge.Should().Be(13);
    }

    [Theory]
    [InlineData(10, false)]
    [InlineData(13, true)]
    [InlineData(16, true)]
    [InlineData(25, true)]
    public void MeetsMinimumAgeRequirement_ShouldReturnCorrectResult(int age, bool expected)
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var dateOfBirth = DateTime.Today.AddYears(-age);
        profile.UpdatePersonalInfo(dateOfBirth, null, null);

        // Act
        var result = profile.MeetsMinimumAgeRequirement();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void MeetsMinimumAgeRequirement_WithoutDateOfBirth_ShouldReturnFalse()
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act
        var result = profile.MeetsMinimumAgeRequirement();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldConvertLanguageToLowerCase()
    {
        // Arrange
        var language = "EN";

        // Act
        var profile = Profile.Create(language);

        // Assert
        profile.PreferredLanguage.Should().Be("en");
    }

    [Fact]
    public void UpdatePersonalInfo_ShouldTrimWhitespaceFromFields()
    {
        // Arrange
        var profile = Profile.CreateDefault();
        var gender = "  Male  ";
        var interests = "  Technology, Sports  ";

        // Act
        profile.UpdatePersonalInfo(null, gender, interests);

        // Assert
        profile.Gender.Should().Be("Male");
        profile.Interests.Should().Be("Technology, Sports");
    }

    [Fact]
    public void UpdatePersonalInfo_WithEmptyStrings_ShouldSetToNull()
    {
        // Arrange
        var profile = Profile.CreateDefault();

        // Act
        profile.UpdatePersonalInfo(null, "", "   ");

        // Assert
        profile.Gender.Should().BeNull();
        profile.Interests.Should().BeNull();
    }
}
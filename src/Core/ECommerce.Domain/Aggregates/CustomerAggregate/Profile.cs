namespace ECommerce.Domain.Aggregates.CustomerAggregate;

/// <summary>
/// Represents customer preferences for communication
/// </summary>
public enum CommunicationPreference
{
    Email,
    SMS,
    Phone,
    None
}

/// <summary>
/// Represents customer's preferred currency
/// </summary>
public enum PreferredCurrency
{
    USD,
    EUR,
    GBP,
    CAD,
    AUD,
    JPY
}

/// <summary>
/// Profile entity containing customer preferences and settings
/// </summary>
public class Profile : Entity
{
    /// <summary>
    /// Customer's preferred language (ISO 639-1 code)
    /// </summary>
    public string PreferredLanguage { get; private set; } = "en";

    /// <summary>
    /// Customer's preferred currency
    /// </summary>
    public PreferredCurrency PreferredCurrency { get; private set; } = PreferredCurrency.USD;

    /// <summary>
    /// Customer's timezone (IANA timezone identifier)
    /// </summary>
    public string Timezone { get; private set; } = "UTC";

    /// <summary>
    /// Customer's preferred communication method
    /// </summary>
    public CommunicationPreference CommunicationPreference { get; private set; } = CommunicationPreference.Email;

    /// <summary>
    /// Whether the customer wants to receive marketing emails
    /// </summary>
    public bool ReceiveMarketingEmails { get; private set; } = true;

    /// <summary>
    /// Whether the customer wants to receive order notifications
    /// </summary>
    public bool ReceiveOrderNotifications { get; private set; } = true;

    /// <summary>
    /// Whether the customer wants to receive promotional SMS
    /// </summary>
    public bool ReceivePromotionalSms { get; private set; } = false;

    /// <summary>
    /// Customer's date of birth (optional)
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    /// <summary>
    /// Customer's gender (optional)
    /// </summary>
    public string? Gender { get; private set; }

    /// <summary>
    /// Customer's interests or preferences (JSON or comma-separated)
    /// </summary>
    public string? Interests { get; private set; }

    /// <summary>
    /// Customer's loyalty tier or membership level
    /// </summary>
    public string LoyaltyTier { get; private set; } = "Bronze";

    /// <summary>
    /// Customer's accumulated loyalty points
    /// </summary>
    public int LoyaltyPoints { get; private set; } = 0;

    /// <summary>
    /// Whether the customer account is verified (email verification)
    /// </summary>
    public bool IsVerified { get; private set; } = false;

    /// <summary>
    /// Date when the customer was last verified
    /// </summary>
    public DateTime? VerificationDate { get; private set; }

    /// <summary>
    /// Customer's age (calculated from date of birth)
    /// </summary>
    public int? Age
    {
        get
        {
            if (!DateOfBirth.HasValue)
                return null;

            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            
            if (DateOfBirth.Value.Date > today.AddYears(-age))
                age--;
            
            return age;
        }
    }

    /// <summary>
    /// Checks if the customer is eligible for adult content/products
    /// </summary>
    public bool IsAdult => Age >= 18;

    /// <summary>
    /// Gets the customer's preferred currency symbol
    /// </summary>
    public string CurrencySymbol => PreferredCurrency switch
    {
        PreferredCurrency.USD => "$",
        PreferredCurrency.EUR => "€",
        PreferredCurrency.GBP => "£",
        PreferredCurrency.CAD => "C$",
        PreferredCurrency.AUD => "A$",
        PreferredCurrency.JPY => "¥",
        _ => "$"
    };

    // Private constructor for EF Core
    private Profile() { }

    /// <summary>
    /// Creates a default profile with standard settings
    /// </summary>
    /// <returns>A new Profile instance with default values</returns>
    public static Profile CreateDefault()
    {
        return new Profile
        {
            PreferredLanguage = "en",
            PreferredCurrency = PreferredCurrency.USD,
            Timezone = "UTC",
            CommunicationPreference = CommunicationPreference.Email,
            ReceiveMarketingEmails = true,
            ReceiveOrderNotifications = true,
            ReceivePromotionalSms = false,
            LoyaltyTier = "Bronze",
            LoyaltyPoints = 0,
            IsVerified = false
        };
    }

    /// <summary>
    /// Creates a profile with custom settings
    /// </summary>
    /// <param name="preferredLanguage">Preferred language code</param>
    /// <param name="preferredCurrency">Preferred currency</param>
    /// <param name="timezone">Timezone identifier</param>
    /// <param name="communicationPreference">Communication preference</param>
    /// <returns>A new Profile instance</returns>
    public static Profile Create(
        string preferredLanguage = "en",
        PreferredCurrency preferredCurrency = PreferredCurrency.USD,
        string timezone = "UTC",
        CommunicationPreference communicationPreference = CommunicationPreference.Email)
    {
        ValidateProfileCreation(preferredLanguage, timezone);

        return new Profile
        {
            PreferredLanguage = preferredLanguage.ToLowerInvariant(),
            PreferredCurrency = preferredCurrency,
            Timezone = timezone,
            CommunicationPreference = communicationPreference,
            ReceiveMarketingEmails = true,
            ReceiveOrderNotifications = true,
            ReceivePromotionalSms = false,
            LoyaltyTier = "Bronze",
            LoyaltyPoints = 0,
            IsVerified = false
        };
    }

    /// <summary>
    /// Updates the customer's language and localization preferences
    /// </summary>
    /// <param name="preferredLanguage">New preferred language</param>
    /// <param name="preferredCurrency">New preferred currency</param>
    /// <param name="timezone">New timezone</param>
    public void UpdateLocalizationPreferences(
        string preferredLanguage,
        PreferredCurrency preferredCurrency,
        string timezone)
    {
        ValidateLocalizationUpdate(preferredLanguage, timezone);

        PreferredLanguage = preferredLanguage.ToLowerInvariant();
        PreferredCurrency = preferredCurrency;
        Timezone = timezone;

        MarkAsModified();
    }

    /// <summary>
    /// Updates the customer's communication preferences
    /// </summary>
    /// <param name="communicationPreference">Preferred communication method</param>
    /// <param name="receiveMarketingEmails">Whether to receive marketing emails</param>
    /// <param name="receiveOrderNotifications">Whether to receive order notifications</param>
    /// <param name="receivePromotionalSms">Whether to receive promotional SMS</param>
    public void UpdateCommunicationPreferences(
        CommunicationPreference communicationPreference,
        bool receiveMarketingEmails,
        bool receiveOrderNotifications,
        bool receivePromotionalSms)
    {
        CommunicationPreference = communicationPreference;
        ReceiveMarketingEmails = receiveMarketingEmails;
        ReceiveOrderNotifications = receiveOrderNotifications;
        ReceivePromotionalSms = receivePromotionalSms;

        MarkAsModified();
    }

    /// <summary>
    /// Updates the customer's personal information
    /// </summary>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <param name="gender">Gender</param>
    /// <param name="interests">Customer interests</param>
    public void UpdatePersonalInfo(DateTime? dateOfBirth, string? gender, string? interests)
    {
        if (dateOfBirth.HasValue)
        {
            ValidateDateOfBirth(dateOfBirth.Value);
        }

        if (!string.IsNullOrWhiteSpace(gender) && gender.Trim().Length > 20)
            throw new ArgumentException("Gender cannot exceed 20 characters.", nameof(gender));

        if (!string.IsNullOrWhiteSpace(interests) && interests.Trim().Length > 500)
            throw new ArgumentException("Interests cannot exceed 500 characters.", nameof(interests));

        DateOfBirth = dateOfBirth;
        Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();
        Interests = string.IsNullOrWhiteSpace(interests) ? null : interests.Trim();

        MarkAsModified();
    }

    /// <summary>
    /// Adds loyalty points to the customer's account
    /// </summary>
    /// <param name="points">Points to add</param>
    public void AddLoyaltyPoints(int points)
    {
        if (points < 0)
            throw new ArgumentException("Points to add cannot be negative.", nameof(points));

        LoyaltyPoints += points;
        UpdateLoyaltyTier();
        MarkAsModified();
    }

    /// <summary>
    /// Redeems loyalty points from the customer's account
    /// </summary>
    /// <param name="points">Points to redeem</param>
    public void RedeemLoyaltyPoints(int points)
    {
        if (points < 0)
            throw new ArgumentException("Points to redeem cannot be negative.", nameof(points));

        if (points > LoyaltyPoints)
            throw new InvalidOperationException("Cannot redeem more points than available.");

        LoyaltyPoints -= points;
        UpdateLoyaltyTier();
        MarkAsModified();
    }

    /// <summary>
    /// Marks the customer profile as verified
    /// </summary>
    public void MarkAsVerified()
    {
        if (IsVerified)
            return;

        IsVerified = true;
        VerificationDate = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// Marks the customer profile as unverified
    /// </summary>
    public void MarkAsUnverified()
    {
        if (!IsVerified)
            return;

        IsVerified = false;
        VerificationDate = null;
        MarkAsModified();
    }

    /// <summary>
    /// Updates the loyalty tier based on current points
    /// </summary>
    private void UpdateLoyaltyTier()
    {
        var newTier = LoyaltyPoints switch
        {
            >= 10000 => "Platinum",
            >= 5000 => "Gold",
            >= 1000 => "Silver",
            _ => "Bronze"
        };

        if (LoyaltyTier != newTier)
        {
            LoyaltyTier = newTier;
        }
    }

    /// <summary>
    /// Checks if the customer can receive communications via the specified method
    /// </summary>
    /// <param name="method">Communication method to check</param>
    /// <returns>True if the customer can receive communications via the specified method</returns>
    public bool CanReceiveCommunication(CommunicationPreference method)
    {
        return method switch
        {
            CommunicationPreference.Email => ReceiveMarketingEmails || ReceiveOrderNotifications,
            CommunicationPreference.SMS => ReceivePromotionalSms,
            CommunicationPreference.Phone => CommunicationPreference == CommunicationPreference.Phone,
            CommunicationPreference.None => false,
            _ => false
        };
    }

    /// <summary>
    /// Gets the minimum age required for the customer's country/region
    /// </summary>
    /// <returns>Minimum age requirement</returns>
    public int GetMinimumAgeRequirement()
    {
        // This could be expanded to consider different countries/regions
        return 13; // COPPA compliance for US
    }

    /// <summary>
    /// Checks if the customer meets the minimum age requirement
    /// </summary>
    public bool MeetsMinimumAgeRequirement()
    {
        return Age >= GetMinimumAgeRequirement();
    }

    private static void ValidateProfileCreation(string preferredLanguage, string timezone)
    {
        ValidateLocalizationUpdate(preferredLanguage, timezone);
    }

    private static void ValidateLocalizationUpdate(string preferredLanguage, string timezone)
    {
        if (string.IsNullOrWhiteSpace(preferredLanguage))
            throw new ArgumentException("Preferred language cannot be null or empty.", nameof(preferredLanguage));

        if (string.IsNullOrWhiteSpace(timezone))
            throw new ArgumentException("Timezone cannot be null or empty.", nameof(timezone));

        if (preferredLanguage.Trim().Length != 2)
            throw new ArgumentException("Preferred language must be a 2-character ISO 639-1 code.", nameof(preferredLanguage));

        if (timezone.Trim().Length > 50)
            throw new ArgumentException("Timezone cannot exceed 50 characters.", nameof(timezone));
    }

    private static void ValidateDateOfBirth(DateTime dateOfBirth)
    {
        if (dateOfBirth > DateTime.Today)
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));

        if (dateOfBirth < DateTime.Today.AddYears(-150))
            throw new ArgumentException("Date of birth cannot be more than 150 years ago.", nameof(dateOfBirth));
    }
}
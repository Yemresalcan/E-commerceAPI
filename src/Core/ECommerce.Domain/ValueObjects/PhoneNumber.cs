using System.Text.RegularExpressions;

namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Represents a phone number with format validation.
/// </summary>
public record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    private static readonly Regex DigitsOnlyRegex = new(
        @"\D",
        RegexOptions.Compiled);

    public string Value { get; }
    public string CountryCode { get; }
    public string Number { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(value));

        // Pre-validation: check for invalid characters that shouldn't be in a phone number
        if (ContainsInvalidCharacters(value))
            throw new ArgumentException("Invalid phone number format.", nameof(value));

        var normalizedValue = NormalizePhoneNumber(value);

        if (string.IsNullOrEmpty(normalizedValue) || !IsValidPhoneNumber(normalizedValue))
            throw new ArgumentException("Invalid phone number format.", nameof(value));

        Value = normalizedValue;
        (CountryCode, Number) = ExtractCountryCodeAndNumber(normalizedValue);
    }

    /// <summary>
    /// Gets the phone number formatted for display.
    /// </summary>
    public string FormattedValue
    {
        get
        {
            if (CountryCode == "1" && Number.Length == 10) // US/Canada format
            {
                return $"+1 ({Number[..3]}) {Number[3..6]}-{Number[6..]}";
            }
            
            return Value.StartsWith('+') ? Value : $"+{Value}";
        }
    }

    /// <summary>
    /// Checks if the phone number contains invalid characters that shouldn't be in a phone number.
    /// </summary>
    private static bool ContainsInvalidCharacters(string phoneNumber)
    {
        // Allow only digits, +, -, (, ), spaces, and dots
        var allowedChars = new HashSet<char> { '+', '-', '(', ')', ' ', '.' };
        
        foreach (char c in phoneNumber)
        {
            if (!char.IsDigit(c) && !allowedChars.Contains(c))
                return true;
        }
        
        return false;
    }

    /// <summary>
    /// Normalizes the phone number by removing non-digit characters except the leading +.
    /// </summary>
    private static string NormalizePhoneNumber(string phoneNumber)
    {
        var trimmed = phoneNumber.Trim();
        
        // Handle leading + sign
        var hasPlus = trimmed.StartsWith('+');
        var digitsOnly = DigitsOnlyRegex.Replace(trimmed, "");
        
        // Check for invalid characters that would result in empty digits
        if (string.IsNullOrEmpty(digitsOnly))
            return "";
        
        // Check for multiple plus signs or invalid format
        if (trimmed.Count(c => c == '+') > 1)
            return "";
        
        if (hasPlus && digitsOnly.Length > 0)
        {
            return $"+{digitsOnly}";
        }
        
        return digitsOnly;
    }

    /// <summary>
    /// Validates if the provided string is a valid phone number format.
    /// </summary>
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Check if normalized phone number is empty (happens when input contains only letters)
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        // Must match the basic pattern
        if (!PhoneRegex.IsMatch(phoneNumber))
            return false;

        // Extract digits only for length validation
        var digitsOnly = phoneNumber.StartsWith('+') ? phoneNumber[1..] : phoneNumber;
        
        // Phone number should be between 7 and 15 digits (ITU-T E.164 standard)
        if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
            return false;

        // Additional validation: cannot start with 0 after country code
        if (digitsOnly.StartsWith('0'))
            return false;

        // Ensure all characters after + are digits
        if (!digitsOnly.All(char.IsDigit))
            return false;

        return true;
    }

    /// <summary>
    /// Extracts country code and number from the normalized phone number.
    /// </summary>
    private static (string CountryCode, string Number) ExtractCountryCodeAndNumber(string normalizedPhone)
    {
        var digitsOnly = normalizedPhone.StartsWith('+') ? normalizedPhone[1..] : normalizedPhone;
        
        // Simple country code extraction (this is a simplified version)
        // In a real-world scenario, you'd use a comprehensive country code database
        if (digitsOnly.Length >= 10 && (digitsOnly.StartsWith('1'))) // US/Canada
        {
            return ("1", digitsOnly[1..]);
        }
        
        if (digitsOnly.Length >= 10 && digitsOnly.StartsWith("44")) // UK
        {
            return ("44", digitsOnly[2..]);
        }
        
        if (digitsOnly.Length >= 10 && digitsOnly.StartsWith("49")) // Germany
        {
            return ("49", digitsOnly[2..]);
        }
        
        // Default: assume first 1-3 digits are country code
        if (digitsOnly.Length >= 10)
        {
            // Try 3-digit country code first
            if (digitsOnly.Length >= 12)
                return (digitsOnly[..3], digitsOnly[3..]);
            
            // Try 2-digit country code
            if (digitsOnly.Length >= 11)
                return (digitsOnly[..2], digitsOnly[2..]);
            
            // For 10-digit numbers starting with 1, treat as US/Canada
            if (digitsOnly.Length == 10 && digitsOnly.StartsWith('1'))
                return ("1", digitsOnly[1..]);
        }
        
        // For domestic numbers without country code, return the full number as the number part
        return ("", digitsOnly);
    }

    /// <summary>
    /// Checks if this is a mobile number (simplified check).
    /// </summary>
    public bool IsMobile
    {
        get
        {
            // Simplified mobile detection - in reality, this would be more complex
            // and would depend on country-specific rules
            return CountryCode switch
            {
                "1" => Number.Length == 10 && (Number.StartsWith('2') || Number.StartsWith('3') || 
                                              Number.StartsWith('4') || Number.StartsWith('5') ||
                                              Number.StartsWith('6') || Number.StartsWith('7') ||
                                              Number.StartsWith('8') || Number.StartsWith('9')),
                "44" => Number.StartsWith('7'),
                _ => true // Default to mobile for unknown country codes
            };
        }
    }

    public override string ToString() => FormattedValue;

    // Implicit conversion from string for convenience
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
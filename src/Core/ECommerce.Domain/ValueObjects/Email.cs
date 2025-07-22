using System.Text.RegularExpressions;

namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Represents an email address with validation logic.
/// </summary>
public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty.", nameof(value));

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > 254) // RFC 5321 limit
            throw new ArgumentException("Email address is too long. Maximum length is 254 characters.", nameof(value));

        if (!IsValidEmail(trimmedValue))
            throw new ArgumentException("Invalid email format.", nameof(value));

        Value = trimmedValue.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the local part of the email address (before @).
    /// </summary>
    public string LocalPart => Value.Split('@')[0];

    /// <summary>
    /// Gets the domain part of the email address (after @).
    /// </summary>
    public string Domain => Value.Split('@')[1];

    /// <summary>
    /// Validates if the provided string is a valid email format.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Basic regex validation
        if (!EmailRegex.IsMatch(email))
            return false;

        // Additional validation rules
        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var localPart = parts[0];
        var domain = parts[1];

        // Local part validation
        if (localPart.Length == 0 || localPart.Length > 64) // RFC 5321 limit
            return false;

        if (localPart.StartsWith('.') || localPart.EndsWith('.'))
            return false;

        if (localPart.Contains(".."))
            return false;

        // Domain validation
        if (domain.Length == 0 || domain.Length > 253) // RFC 5321 limit
            return false;

        if (domain.StartsWith('.') || domain.EndsWith('.'))
            return false;

        if (domain.StartsWith('-') || domain.EndsWith('-'))
            return false;

        if (domain.Contains(".."))
            return false;

        // Domain must contain at least one dot
        if (!domain.Contains('.'))
            return false;

        // Check for invalid domain patterns
        if (domain.EndsWith("-.") || domain.Contains("-."))
            return false;

        // Check each domain part
        var domainParts = domain.Split('.');
        foreach (var part in domainParts)
        {
            if (string.IsNullOrEmpty(part))
                return false;
            
            if (part.StartsWith('-') || part.EndsWith('-'))
                return false;
        }

        return true;
    }

    public override string ToString() => Value;

    // Implicit conversion from string for convenience
    public static implicit operator string(Email email) => email.Value;
}
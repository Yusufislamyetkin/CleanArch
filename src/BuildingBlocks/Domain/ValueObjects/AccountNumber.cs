using System.Text.RegularExpressions;

namespace Enterprise.BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// Account number value object
/// Represents a bank account number with validation
/// </summary>
public sealed class AccountNumber : IEquatable<AccountNumber>
{
    private const int MinLength = 8;
    private const int MaxLength = 34;
    private static readonly Regex ValidPattern = new(@"^[A-Z0-9]+$", RegexOptions.Compiled);

    public string Value { get; }

    private AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account number cannot be empty", nameof(value));

        var cleanValue = value.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        if (cleanValue.Length < MinLength || cleanValue.Length > MaxLength)
            throw new ArgumentException($"Account number must be between {MinLength} and {MaxLength} characters", nameof(value));

        if (!ValidPattern.IsMatch(cleanValue))
            throw new ArgumentException("Account number can only contain letters and numbers", nameof(value));

        Value = cleanValue;
    }

    public static AccountNumber From(string value) => new(value);

    public bool IsValidLength => Value.Length >= MinLength && Value.Length <= MaxLength;

    public string GetMaskedValue()
    {
        if (Value.Length <= 4) return Value;
        return $"****{Value[^4..]}"; // Last 4 characters
    }

    public override bool Equals(object? obj) => Equals(obj as AccountNumber);

    public bool Equals(AccountNumber? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(AccountNumber? left, AccountNumber? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(AccountNumber? left, AccountNumber? right) => !(left == right);
}

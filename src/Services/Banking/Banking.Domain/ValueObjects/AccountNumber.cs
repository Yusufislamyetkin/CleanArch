using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.ValueObjects;

/// <summary>
/// Account number value object
/// Represents unique account identifiers with validation
/// </summary>
public sealed class AccountNumber : ValueObject
{
    public string Value { get; private set; }

    private AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account number cannot be empty", nameof(value));

        if (!IsValidAccountNumber(value))
            throw new ArgumentException("Invalid account number format", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Factory method to create AccountNumber
    /// </summary>
    public static AccountNumber From(string value)
    {
        return new AccountNumber(value);
    }

    /// <summary>
    /// Validate account number format
    /// Format: XX followed by 12 digits (e.g., 1023456789012)
    /// </summary>
    private static bool IsValidAccountNumber(string value)
    {
        if (value.Length != 14)
            return false;

        // First 2 characters should be digits (account type prefix)
        if (!char.IsDigit(value[0]) || !char.IsDigit(value[1]))
            return false;

        // Remaining 12 characters should be digits
        for (int i = 2; i < value.Length; i++)
        {
            if (!char.IsDigit(value[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get account type from prefix
    /// </summary>
    public string GetAccountTypePrefix()
    {
        return Value.Substring(0, 2);
    }

    /// <summary>
    /// Check if this is a checking account
    /// </summary>
    public bool IsCheckingAccount => GetAccountTypePrefix() == "10";

    /// <summary>
    /// Check if this is a savings account
    /// </summary>
    public bool IsSavingsAccount => GetAccountTypePrefix() == "20";

    /// <summary>
    /// Check if this is an investment account
    /// </summary>
    public bool IsInvestmentAccount => GetAccountTypePrefix() == "30";

    /// <summary>
    /// Check if this is a credit account
    /// </summary>
    public bool IsCreditAccount => GetAccountTypePrefix() == "40";

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}

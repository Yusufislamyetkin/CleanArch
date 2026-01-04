using Enterprise.BuildingBlocks.Domain.Entities;
using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Model;

/// <summary>
/// Account activity entity
/// Tracks all activities performed on an account
/// </summary>
public class AccountActivity : Entity<Guid>
{
    /// <summary>
    /// Account ID this activity belongs to
    /// </summary>
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Activity description
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Activity amount (can be zero for non-financial activities)
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// Balance after activity
    /// </summary>
    public Money BalanceAfter { get; private set; }

    /// <summary>
    /// Balance before activity
    /// </summary>
    public Money? BalanceBefore { get; private set; }

    /// <summary>
    /// Activity timestamp
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Activity type
    /// </summary>
    public ActivityType Type { get; private set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; private set; }

    // Private constructor for EF Core
    private AccountActivity() : base()
    {
        Description = string.Empty;
        Amount = Money.Zero("TRY");
        BalanceAfter = Money.Zero("TRY");
        Type = ActivityType.Other;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new account activity
    /// </summary>
    public AccountActivity(
        Guid id,
        Guid accountId,
        string description,
        Money amount,
        Money balanceAfter,
        Money? balanceBefore = null,
        DateTime? timestamp = null)
        : base()
    {
        Id = id;
        AccountId = accountId;
        Description = description;
        Amount = amount;
        BalanceAfter = balanceAfter;
        BalanceBefore = balanceBefore;
        Timestamp = timestamp ?? DateTime.UtcNow;
        Type = DetermineActivityType(description, amount);
    }

    /// <summary>
    /// Creates a financial activity (deposit/withdrawal)
    /// </summary>
    public static AccountActivity CreateFinancialActivity(
        Guid accountId,
        string description,
        Money amount,
        Money balanceAfter,
        Money balanceBefore)
    {
        return new AccountActivity(
            Guid.NewGuid(),
            accountId,
            description,
            amount,
            balanceAfter,
            balanceBefore);
    }

    /// <summary>
    /// Creates a non-financial activity (name change, status change, etc.)
    /// </summary>
    public static AccountActivity CreateAdministrativeActivity(
        Guid accountId,
        string description,
        Money balanceAfter)
    {
        return new AccountActivity(
            Guid.NewGuid(),
            accountId,
            description,
            Money.Zero(balanceAfter.Currency),
            balanceAfter);
    }

    /// <summary>
    /// Adds metadata to the activity
    /// </summary>
    public void AddMetadata(object metadata)
    {
        Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
    }

    /// <summary>
    /// Gets metadata as typed object
    /// </summary>
    public T? GetMetadata<T>() where T : class
    {
        if (string.IsNullOrEmpty(Metadata))
            return null;

        return System.Text.Json.JsonSerializer.Deserialize<T>(Metadata);
    }

    private ActivityType DetermineActivityType(string description, Money amount)
    {
        var desc = description.ToLowerInvariant();

        if (desc.Contains("deposit") || (amount.Amount > 0 && !desc.Contains("fee")))
            return ActivityType.Deposit;

        if (desc.Contains("withdraw") || amount.Amount < 0)
            return ActivityType.Withdrawal;

        if (desc.Contains("transfer"))
            return ActivityType.Transfer;

        if (desc.Contains("fee") || desc.Contains("charge"))
            return ActivityType.Fee;

        if (desc.Contains("name") || desc.Contains("update"))
            return ActivityType.Administrative;

        if (desc.Contains("create") || desc.Contains("open"))
            return ActivityType.Administrative;

        if (desc.Contains("close"))
            return ActivityType.Administrative;

        if (desc.Contains("freeze") || desc.Contains("unfreeze"))
            return ActivityType.Administrative;

        return ActivityType.Other;
    }

    /// <summary>
    /// Gets the balance change amount
    /// </summary>
    public Money GetBalanceChange()
    {
        if (BalanceBefore.HasValue)
        {
            return BalanceAfter.Subtract(BalanceBefore.Value);
        }

        return Amount; // Fallback to amount
    }

    /// <summary>
    /// Checks if this is a financial activity
    /// </summary>
    public bool IsFinancialActivity()
    {
        return Type is ActivityType.Deposit or ActivityType.Withdrawal
            or ActivityType.Transfer or ActivityType.Fee;
    }

    /// <summary>
    /// Checks if this is an administrative activity
    /// </summary>
    public bool IsAdministrativeActivity()
    {
        return Type == ActivityType.Administrative;
    }
}

/// <summary>
/// Activity type enumeration
/// </summary>
public enum ActivityType
{
    Deposit,
    Withdrawal,
    Transfer,
    Fee,
    Administrative,
    Other
}

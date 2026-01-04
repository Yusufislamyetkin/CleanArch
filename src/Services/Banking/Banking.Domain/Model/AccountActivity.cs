using Enterprise.BuildingBlocks.Domain.Entities;

namespace Enterprise.Services.Banking.Domain.Model;

/// <summary>
/// Account activity entity
/// Represents audit trail and activity log for accounts
/// </summary>
public class AccountActivity : Entity<Guid>
{
    public Guid AccountId { get; private set; }
    public string ActivityType { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime Timestamp { get; private set; }
    public string? AdditionalData { get; private set; }

    // Navigation property
    public Account Account { get; private set; } = null!;

    // Private constructor for EF Core
    private AccountActivity() : base() { }

    /// <summary>
    /// Create a new account activity
    /// </summary>
    public static AccountActivity Create(
        Guid activityId,
        Guid accountId,
        string activityType,
        string description,
        DateTime timestamp,
        string? additionalData = null)
    {
        return new AccountActivity
        {
            Id = activityId,
            AccountId = accountId,
            ActivityType = activityType,
            Description = description,
            Timestamp = timestamp,
            AdditionalData = additionalData,
            CreatedAt = timestamp
        };
    }
}

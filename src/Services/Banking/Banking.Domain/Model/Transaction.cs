using Enterprise.BuildingBlocks.Domain.Entities;
using Enterprise.Services.Banking.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Model;

/// <summary>
/// Transaction entity
/// Represents financial transactions on accounts
/// </summary>
public class Transaction : Entity<Guid>
{
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public Money Amount { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTime Timestamp { get; private set; }
    public string? ExternalReference { get; private set; }

    // Navigation property
    public Account Account { get; private set; } = null!;

    // Private constructor for EF Core
    private Transaction() : base() { }

    /// <summary>
    /// Create a deposit transaction
    /// </summary>
    public static Transaction CreateDeposit(
        Guid transactionId,
        Guid accountId,
        Money amount,
        string description,
        DateTime timestamp)
    {
        return new Transaction
        {
            Id = transactionId,
            AccountId = accountId,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Description = description,
            Timestamp = timestamp,
            CreatedAt = timestamp
        };
    }

    /// <summary>
    /// Create a withdrawal transaction
    /// </summary>
    public static Transaction CreateWithdrawal(
        Guid transactionId,
        Guid accountId,
        Money amount,
        string description,
        DateTime timestamp)
    {
        return new Transaction
        {
            Id = transactionId,
            AccountId = accountId,
            Type = TransactionType.Withdrawal,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Description = description,
            Timestamp = timestamp,
            CreatedAt = timestamp
        };
    }

    /// <summary>
    /// Create a transfer in transaction
    /// </summary>
    public static Transaction CreateTransferIn(
        Guid transactionId,
        Guid accountId,
        Money amount,
        string description,
        DateTime timestamp,
        string externalReference)
    {
        return new Transaction
        {
            Id = transactionId,
            AccountId = accountId,
            Type = TransactionType.TransferIn,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Description = description,
            Timestamp = timestamp,
            ExternalReference = externalReference,
            CreatedAt = timestamp
        };
    }

    /// <summary>
    /// Create a transfer out transaction
    /// </summary>
    public static Transaction CreateTransferOut(
        Guid transactionId,
        Guid accountId,
        Money amount,
        string description,
        DateTime timestamp,
        string externalReference)
    {
        return new Transaction
        {
            Id = transactionId,
            AccountId = accountId,
            Type = TransactionType.TransferOut,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Description = description,
            Timestamp = timestamp,
            ExternalReference = externalReference,
            CreatedAt = timestamp
        };
    }

    /// <summary>
    /// Create a fee transaction
    /// </summary>
    public static Transaction CreateFee(
        Guid transactionId,
        Guid accountId,
        Money amount,
        string description,
        DateTime timestamp)
    {
        return new Transaction
        {
            Id = transactionId,
            AccountId = accountId,
            Type = TransactionType.Fee,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Description = description,
            Timestamp = timestamp,
            CreatedAt = timestamp
        };
    }

    /// <summary>
    /// Create an interest transaction
    /// </summary>
    public static Transaction CreateInterest(
        Guid transactionId,
        Guid accountId,
        Money amount,
        string description,
        DateTime timestamp)
    {
        return new Transaction
        {
            Id = transactionId,
            AccountId = accountId,
            Type = TransactionType.Interest,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Description = description,
            Timestamp = timestamp,
            CreatedAt = timestamp
        };
    }

    /// <summary>
    /// Mark transaction as failed
    /// </summary>
    public void MarkAsFailed()
    {
        Status = TransactionStatus.Failed;
    }

    /// <summary>
    /// Mark transaction as cancelled
    /// </summary>
    public void MarkAsCancelled()
    {
        Status = TransactionStatus.Cancelled;
    }

    /// <summary>
    /// Update external reference
    /// </summary>
    public void UpdateExternalReference(string externalReference)
    {
        ExternalReference = externalReference;
    }
}

/// <summary>
/// Transaction type enumeration
/// </summary>
public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    TransferIn = 3,
    TransferOut = 4,
    Fee = 5,
    Interest = 6
}

/// <summary>
/// Transaction status enumeration
/// </summary>
public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

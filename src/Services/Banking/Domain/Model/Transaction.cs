using Enterprise.BuildingBlocks.Domain.Entities;
using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Model;

/// <summary>
/// Transaction entity representing a financial transaction
/// Part of Account aggregate
/// </summary>
public class Transaction : Entity<Guid>
{
    /// <summary>
    /// Account ID this transaction belongs to
    /// </summary>
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Transaction amount
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// Transaction type
    /// </summary>
    public TransactionType Type { get; private set; }

    /// <summary>
    /// Transaction status
    /// </summary>
    public TransactionStatus Status { get; private set; }

    /// <summary>
    /// Transaction timestamp
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Transaction description
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// External reference (for integration with external systems)
    /// </summary>
    public string? ExternalReference { get; private set; }

    /// <summary>
    /// Related transaction ID (for transfers)
    /// </summary>
    public Guid? RelatedTransactionId { get; private set; }

    /// <summary>
    /// Processing timestamp
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    // Private constructor for EF Core
    private Transaction() : base()
    {
        Description = string.Empty;
        Amount = Money.Zero("TRY");
        Type = TransactionType.Deposit;
        Status = TransactionStatus.Pending;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a deposit transaction
    /// </summary>
    public static Transaction CreateDeposit(Guid accountId, Money amount, string description = "")
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Description = description,
            Timestamp = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a withdrawal transaction
    /// </summary>
    public static Transaction CreateWithdrawal(Guid accountId, Money amount, string description = "")
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = new Money(-amount.Amount, amount.Currency), // Negative for withdrawal
            Type = TransactionType.Withdrawal,
            Status = TransactionStatus.Completed,
            Description = description,
            Timestamp = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a transfer transaction
    /// </summary>
    public static Transaction CreateTransfer(
        Guid accountId,
        Guid targetAccountId,
        Money amount,
        string description = "")
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = new Money(-amount.Amount, amount.Currency), // Negative for outgoing transfer
            Type = TransactionType.Transfer,
            Status = TransactionStatus.Completed,
            Description = description,
            Timestamp = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            RelatedTransactionId = targetAccountId
        };
    }

    /// <summary>
    /// Creates a fee transaction
    /// </summary>
    public static Transaction CreateFee(Guid accountId, Money amount, string description = "")
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Amount = new Money(-amount.Amount, amount.Currency), // Negative for fee
            Type = TransactionType.Fee,
            Status = TransactionStatus.Completed,
            Description = description,
            Timestamp = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks transaction as failed
    /// </summary>
    public void MarkAsFailed(string reason = "")
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed transaction");

        Status = TransactionStatus.Failed;
        if (!string.IsNullOrEmpty(reason))
        {
            Description += $" (Failed: {reason})";
        }
    }

    /// <summary>
    /// Marks transaction as processing
    /// </summary>
    public void MarkAsProcessing()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be marked as processing");

        Status = TransactionStatus.Processing;
    }

    /// <summary>
    /// Marks transaction as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidOperationException("Transaction is already completed");

        Status = TransactionStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets external reference
    /// </summary>
    public void SetExternalReference(string reference)
    {
        ExternalReference = reference;
    }

    /// <summary>
    /// Gets the absolute transaction amount (always positive)
    /// </summary>
    public Money GetAbsoluteAmount()
    {
        return new Money(Math.Abs(Amount.Amount), Amount.Currency);
    }

    /// <summary>
    /// Checks if transaction is debit (money going out)
    /// </summary>
    public bool IsDebit()
    {
        return Amount.Amount < 0;
    }

    /// <summary>
    /// Checks if transaction is credit (money coming in)
    /// </summary>
    public bool IsCredit()
    {
        return Amount.Amount > 0;
    }
}

/// <summary>
/// Transaction type enumeration
/// </summary>
public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    Fee,
    Interest,
    Adjustment
}

/// <summary>
/// Transaction status enumeration
/// </summary>
public enum TransactionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
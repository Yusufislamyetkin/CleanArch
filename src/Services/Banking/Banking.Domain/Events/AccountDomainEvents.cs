using Enterprise.BuildingBlocks.Domain.Events;
using Enterprise.Services.Banking.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Events;

/// <summary>
/// Domain events for Account aggregate
/// </summary>

/// <summary>
/// Event raised when a new account is created
/// </summary>
public record AccountCreatedDomainEvent(
    Guid AccountId,
    AccountNumber AccountNumber,
    CustomerId CustomerId,
    string AccountName,
    string AccountType,
    Money InitialBalance,
    DateTime CreatedAt) : DomainEvent;

/// <summary>
/// Event raised when money is deposited to an account
/// </summary>
public record MoneyDepositedDomainEvent(
    Guid AccountId,
    Guid TransactionId,
    Money Amount,
    Money NewBalance,
    string Description,
    DateTime DepositedAt) : DomainEvent;

/// <summary>
/// Event raised when money is withdrawn from an account
/// </summary>
public record MoneyWithdrawnDomainEvent(
    Guid AccountId,
    Guid TransactionId,
    Money Amount,
    Money NewBalance,
    string Description,
    DateTime WithdrawnAt) : DomainEvent;

/// <summary>
/// Event raised when money is transferred between accounts
/// </summary>
public record MoneyTransferredDomainEvent(
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    Money FromAccountNewBalance,
    Money ToAccountNewBalance,
    string Description,
    DateTime TransferredAt) : DomainEvent;

/// <summary>
/// Event raised when account name is updated
/// </summary>
public record AccountNameUpdatedDomainEvent(
    Guid AccountId,
    string OldName,
    string NewName,
    DateTime UpdatedAt) : DomainEvent;

/// <summary>
/// Event raised when account is closed
/// </summary>
public record AccountClosedDomainEvent(
    Guid AccountId,
    Money FinalBalance,
    DateTime ClosedAt) : DomainEvent;

/// <summary>
/// Event raised when account status changes
/// </summary>
public record AccountStatusChangedDomainEvent(
    Guid AccountId,
    string OldStatus,
    string NewStatus,
    string Reason,
    DateTime ChangedAt) : DomainEvent;

/// <summary>
/// Event raised when account limits are exceeded
/// </summary>
public record AccountLimitExceededDomainEvent(
    Guid AccountId,
    string LimitType,
    Money AttemptedAmount,
    Money CurrentLimit,
    DateTime ExceededAt) : DomainEvent;

/// <summary>
/// Event raised when daily transaction limit is reached
/// </summary>
public record DailyTransactionLimitReachedDomainEvent(
    Guid AccountId,
    Money DailyLimit,
    Money CurrentTotal,
    DateTime ReachedAt) : DomainEvent;

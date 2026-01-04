using Enterprise.BuildingBlocks.Domain.Events;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Domain.Events;

/// <summary>
/// Event raised when an account is opened
/// </summary>
public record AccountOpenedEvent(
    Guid AccountId,
    AccountNumber AccountNumber,
    Guid CustomerId,
    AccountType AccountType,
    Money InitialBalance,
    DateTime OpenedAt) : DomainEvent
{
    public override string EventType => "AccountOpened";
}

/// <summary>
/// Event raised when money is deposited to an account
/// </summary>
public record MoneyDepositedEvent(
    Guid AccountId,
    Money Amount,
    Money NewBalance,
    Guid TransactionId) : DomainEvent
{
    public override string EventType => "MoneyDeposited";
}

/// <summary>
/// Event raised when money is withdrawn from an account
/// </summary>
public record MoneyWithdrawnEvent(
    Guid AccountId,
    Money Amount,
    Money NewBalance,
    Guid TransactionId) : DomainEvent
{
    public override string EventType => "MoneyWithdrawn";
}

/// <summary>
/// Event raised when money is transferred between accounts
/// </summary>
public record MoneyTransferredEvent(
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    string Description,
    Guid TransferId) : DomainEvent
{
    public override string EventType => "MoneyTransferred";
}

/// <summary>
/// Event raised when account name is updated
/// </summary>
public record AccountNameUpdatedEvent(
    Guid AccountId,
    string OldName,
    string NewName) : DomainEvent
{
    public override string EventType => "AccountNameUpdated";
}

/// <summary>
/// Event raised when an account is closed
/// </summary>
public record AccountClosedEvent(
    Guid AccountId,
    DateTime ClosedAt) : DomainEvent
{
    public override string EventType => "AccountClosed";
}

/// <summary>
/// Event raised when account balance goes below minimum
/// </summary>
public record AccountBalanceLowEvent(
    Guid AccountId,
    Money CurrentBalance,
    Money MinimumBalance) : DomainEvent
{
    public override string EventType => "AccountBalanceLow";
}

/// <summary>
/// Event raised when daily transaction limit is exceeded
/// </summary>
public record DailyLimitExceededEvent(
    Guid AccountId,
    Money AttemptedAmount,
    Money DailyLimit,
    Money CurrentDayTotal) : DomainEvent
{
    public override string EventType => "DailyLimitExceeded";
}

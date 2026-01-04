using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.BuildingBlocks.Domain.Events;

/// <summary>
/// Domain events for Account aggregate
/// </summary>
public record AccountCreatedDomainEvent(
    Guid AccountId,
    CustomerId CustomerId,
    AccountNumber AccountNumber,
    string AccountType,
    Money InitialBalance) : DomainEvent;

public record MoneyDepositedDomainEvent(
    Guid AccountId,
    Money Amount,
    Money NewBalance) : DomainEvent;

public record MoneyWithdrawnDomainEvent(
    Guid AccountId,
    Money Amount,
    Money NewBalance) : DomainEvent;

public record MoneyTransferredDomainEvent(
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    string Description) : DomainEvent;

public record AccountClosedDomainEvent(
    Guid AccountId,
    DateTime ClosedAt) : DomainEvent;

public record AccountFrozenDomainEvent(
    Guid AccountId,
    string Reason) : DomainEvent;

public record AccountUnfrozenDomainEvent(
    Guid AccountId) : DomainEvent;

public record DailyLimitExceededDomainEvent(
    Guid AccountId,
    Money AttemptedAmount,
    Money DailyLimit,
    Money CurrentDayTotal) : DomainEvent;

public record AccountBalanceLowDomainEvent(
    Guid AccountId,
    Money CurrentBalance,
    Money MinimumBalance) : DomainEvent;

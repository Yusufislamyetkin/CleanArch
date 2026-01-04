using Enterprise.BuildingBlocks.Application.Commands;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Application.Commands;

/// <summary>
/// Command to create a new account
/// Write model - contains only data needed for account creation
/// </summary>
public record CreateAccountCommand(
    CustomerId CustomerId,
    AccountNumber AccountNumber,
    string AccountName,
    AccountType AccountType,
    Money InitialBalance,
    Money? MinimumBalance = null,
    Money? DailyTransactionLimit = null,
    string? CorrelationId = null,
    string? TenantId = null) : Command<CreateAccountResult>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Command to deposit money
/// </summary>
public record DepositMoneyCommand(
    Guid AccountId,
    Money Amount,
    string Description = "",
    string? CorrelationId = null,
    string? TenantId = null) : Command<DepositResult>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Command to withdraw money
/// </summary>
public record WithdrawMoneyCommand(
    Guid AccountId,
    Money Amount,
    string Description = "",
    string? CorrelationId = null,
    string? TenantId = null) : Command<WithdrawResult>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Command to transfer money between accounts
/// </summary>
public record TransferMoneyCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    string Description = "",
    string? CorrelationId = null,
    string? TenantId = null) : Command<TransferResult>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Command to update account name
/// </summary>
public record UpdateAccountNameCommand(
    Guid AccountId,
    string NewName,
    string? CorrelationId = null,
    string? TenantId = null) : Command<Unit>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Command to close account
/// </summary>
public record CloseAccountCommand(
    Guid AccountId,
    string? CorrelationId = null,
    string? TenantId = null) : Command<Unit>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Command to freeze account
/// </summary>
public record FreezeAccountCommand(
    Guid AccountId,
    string Reason,
    string? CorrelationId = null,
    string? TenantId = null) : Command<Unit>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Command to unfreeze account
/// </summary>
public record UnfreezeAccountCommand(
    Guid AccountId,
    string? CorrelationId = null,
    string? TenantId = null) : Command<Unit>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

// Result DTOs
public record CreateAccountResult(
    Guid AccountId,
    AccountNumber AccountNumber,
    DateTime CreatedAt);

public record DepositResult(
    Guid AccountId,
    Guid TransactionId,
    Money Amount,
    Money NewBalance);

public record WithdrawResult(
    Guid AccountId,
    Guid TransactionId,
    Money Amount,
    Money NewBalance);

public record TransferResult(
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    Money FromAccountNewBalance,
    Money ToAccountNewBalance);
using Enterprise.BuildingBlocks.Application.Commands;
using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.BuildingBlocks.Application.Commands;

/// <summary>
/// Command to create a new account
/// Write model - optimized for creating accounts
/// </summary>
public record CreateAccountCommand(
    CustomerId CustomerId,
    string AccountName,
    string AccountType,
    Money InitialBalance,
    Money? MinimumBalance = null,
    Money? DailyTransactionLimit = null) : Command<CreateAccountResult>;

/// <summary>
/// Command to deposit money
/// </summary>
public record DepositMoneyCommand(
    Guid AccountId,
    Money Amount,
    string Description = "") : Command<DepositResult>;

/// <summary>
/// Command to withdraw money
/// </summary>
public record WithdrawMoneyCommand(
    Guid AccountId,
    Money Amount,
    string Description = "") : Command<MediatR.Unit>;

/// <summary>
/// Command to transfer money between accounts
/// </summary>
public record TransferMoneyCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    string Description = "") : Command<TransferResult>;

/// <summary>
/// Command to update account name
/// </summary>
public record UpdateAccountNameCommand(
    Guid AccountId,
    string NewName) : Command<MediatR.Unit>;

/// <summary>
/// Command to close account
/// </summary>
public record CloseAccountCommand(
    Guid AccountId) : Command<MediatR.Unit>;

/// <summary>
/// Command to freeze account
/// </summary>
public record FreezeAccountCommand(
    Guid AccountId,
    string Reason) : Command<MediatR.Unit>;

/// <summary>
/// Command to unfreeze account
/// </summary>
public record UnfreezeAccountCommand(
    Guid AccountId) : Command<MediatR.Unit>;

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
using Enterprise.BuildingBlocks.Domain.Services;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Domain.Services;

/// <summary>
/// Account domain service interface
/// Contains business logic that spans multiple aggregates
/// </summary>
public interface IAccountService : IDomainService
{
    /// <summary>
    /// Transfers money between accounts
    /// </summary>
    Task TransferBetweenAccountsAsync(
        Account fromAccount,
        Account toAccount,
        Money amount,
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates account opening rules
    /// </summary>
    Task ValidateAccountOpeningAsync(
        Guid customerId,
        AccountType accountType,
        Money initialBalance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates account fees
    /// </summary>
    Task<Money> CalculateAccountFeesAsync(
        Account account,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks daily transaction limits
    /// </summary>
    Task<bool> CheckDailyTransactionLimitsAsync(
        Account account,
        Money transactionAmount,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Account number generator service
/// </summary>
public interface IAccountNumberGenerator : IDomainService
{
    /// <summary>
    /// Generates a unique account number
    /// </summary>
    Task<AccountNumber> GenerateUniqueAccountNumberAsync(
        AccountType accountType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Account balance service
/// </summary>
public interface IAccountBalanceService : IDomainService
{
    /// <summary>
    /// Calculates available balance (considering holds, pending transactions, etc.)
    /// </summary>
    Task<Money> CalculateAvailableBalanceAsync(
        Account account,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies interest to savings accounts
    /// </summary>
    Task ApplyInterestAsync(
        Account account,
        decimal interestRate,
        CancellationToken cancellationToken = default);
}

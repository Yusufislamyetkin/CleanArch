using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.BuildingBlocks.Domain.Services;

/// <summary>
/// Account domain service interface
/// Contains business logic that spans multiple aggregates
/// </summary>
public interface IAccountDomainService : IDomainService
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
    /// Validates account creation requirements
    /// </summary>
    Task ValidateAccountCreationAsync(
        CustomerId customerId,
        string accountType,
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
    /// Checks if account can perform transaction
    /// </summary>
    Task<bool> CanPerformTransactionAsync(
        Account account,
        Money amount,
        string transactionType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies interest to savings accounts
    /// </summary>
    Task ApplyInterestAsync(
        Account account,
        decimal interestRate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Account number generator service
/// </summary>
public interface IAccountNumberGeneratorDomainService : IDomainService
{
    /// <summary>
    /// Generates a unique account number
    /// </summary>
    Task<AccountNumber> GenerateUniqueAccountNumberAsync(
        string accountType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Account balance service
/// </summary>
public interface IAccountBalanceDomainService : IDomainService
{
    /// <summary>
    /// Calculates available balance
    /// </summary>
    Task<Money> CalculateAvailableBalanceAsync(
        Account account,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if account has sufficient balance
    /// </summary>
    Task<bool> HasSufficientBalanceAsync(
        Account account,
        Money amount,
        CancellationToken cancellationToken = default);
}

// Forward declarations for Account aggregate
public interface Account { }
public interface Transaction { }

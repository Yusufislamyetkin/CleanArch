using Enterprise.BuildingBlocks.Domain.Services;
using Enterprise.Services.Banking.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Services;

/// <summary>
/// Domain service interface for account-related business operations
/// Handles operations that span multiple aggregates or require complex business logic
/// </summary>
public interface IAccountDomainService : IDomainService
{
    /// <summary>
    /// Validate account creation requirements
    /// </summary>
    Task ValidateAccountCreationAsync(
        CustomerId customerId,
        string accountType,
        Money initialBalance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfer money between accounts
    /// </summary>
    Task TransferBetweenAccountsAsync(
        Account fromAccount,
        Account toAccount,
        Money amount,
        string description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate account fees for a given period
    /// </summary>
    Task<Money> CalculateAccountFeesAsync(
        Account account,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a transaction can be performed on an account
    /// </summary>
    Task<bool> CanPerformTransactionAsync(
        Account account,
        Money amount,
        string transactionType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply interest to a savings account
    /// </summary>
    Task ApplyInterestAsync(
        Account account,
        decimal interestRate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Domain service interface for generating unique account numbers
/// </summary>
public interface IAccountNumberGeneratorDomainService : IDomainService
{
    /// <summary>
    /// Generate a unique account number for the given account type
    /// </summary>
    Task<AccountNumber> GenerateUniqueAccountNumberAsync(
        string accountType,
        CancellationToken cancellationToken = default);
}

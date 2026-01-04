using Enterprise.BuildingBlocks.Domain.Services;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Application.Interfaces;
using Enterprise.Services.Banking.Domain.Model;
using Enterprise.Services.Banking.Domain.Services;

namespace Enterprise.Services.Banking.Infrastructure.Services;

/// <summary>
/// Account domain service implementation
/// Contains business logic that spans multiple aggregates
/// </summary>
public class AccountDomainService : IAccountDomainService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountNumberGeneratorDomainService _accountNumberGenerator;

    public AccountDomainService(
        IAccountRepository accountRepository,
        IAccountNumberGeneratorDomainService accountNumberGenerator)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _accountNumberGenerator = accountNumberGenerator ?? throw new ArgumentNullException(nameof(accountNumberGenerator));
    }

    public async Task TransferBetweenAccountsAsync(
        Account fromAccount,
        Account toAccount,
        Money amount,
        string description,
        CancellationToken cancellationToken = default)
    {
        // Validate transfer requirements
        if (fromAccount.Id == toAccount.Id)
            throw new InvalidOperationException("Cannot transfer to the same account");

        if (fromAccount.Type != toAccount.Type)
            throw new InvalidOperationException("Cannot transfer between different account types");

        // Check if both accounts are active
        if (fromAccount.Status != AccountStatus.Active || toAccount.Status != AccountStatus.Active)
            throw new InvalidOperationException("Both accounts must be active for transfer");

        // Check sufficient balance
        if (!fromAccount.HasSufficientBalance(amount))
            throw new InvalidOperationException("Insufficient balance for transfer");

        // Perform the transfer using domain logic
        fromAccount.Withdraw(amount, $"Transfer to {toAccount.AccountNumber}: {description}");
        toAccount.Deposit(amount, $"Transfer from {fromAccount.AccountNumber}: {description}");

        // Update both accounts
        await _accountRepository.UpdateAsync(fromAccount, cancellationToken);
        await _accountRepository.UpdateAsync(toAccount, cancellationToken);
    }

    public async Task ValidateAccountCreationAsync(
        CustomerId customerId,
        string accountType,
        Money initialBalance,
        CancellationToken cancellationToken = default)
    {
        // Check customer doesn't have too many accounts of this type
        var customerAccounts = await _accountRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        var sameTypeCount = customerAccounts.Count(a => a.Type.ToString() == accountType && a.Status == AccountStatus.Active);

        if (accountType == "Checking" && sameTypeCount >= 3)
            throw new InvalidOperationException("Customer cannot have more than 3 checking accounts");

        if (accountType == "Savings" && sameTypeCount >= 2)
            throw new InvalidOperationException("Customer cannot have more than 2 savings accounts");

        // Validate account type specific rules
        switch (accountType)
        {
            case "Savings":
                if (initialBalance.Amount < 100)
                    throw new InvalidOperationException("Savings accounts require minimum 100 TRY initial balance");
                break;

            case "Investment":
                if (initialBalance.Amount < 1000)
                    throw new InvalidOperationException("Investment accounts require minimum 1000 TRY initial balance");
                break;

            case "Credit":
                if (initialBalance.Amount != 0)
                    throw new InvalidOperationException("Credit accounts must start with zero balance");
                break;
        }
    }

    public async Task<Money> CalculateAccountFeesAsync(
        Account account,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        var fees = Money.Zero(account.Balance.Currency);

        // Calculate maintenance fees based on account type
        switch (account.Type)
        {
            case AccountType.Checking:
                // Monthly maintenance fee for checking accounts
                var months = Math.Max(1, (periodEnd - periodStart).Days / 30);
                fees = fees.Add(new Money(5 * months, account.Balance.Currency));
                break;

            case AccountType.Savings:
                // No maintenance fee for savings accounts
                break;

            case AccountType.Investment:
                // Quarterly fee for investment accounts
                var quarters = Math.Max(1, (periodEnd - periodStart).Days / 90);
                fees = fees.Add(new Money(25 * quarters, account.Balance.Currency));
                break;

            case AccountType.Credit:
                // No maintenance fee for credit accounts (interest-based)
                break;
        }

        // Add transaction fees if any
        var transactionFees = await CalculateTransactionFeesAsync(account, periodStart, periodEnd, cancellationToken);
        fees = fees.Add(transactionFees);

        return fees;
    }

    public async Task<bool> CanPerformTransactionAsync(
        Account account,
        Money amount,
        string transactionType,
        CancellationToken cancellationToken = default)
    {
        // Check account status
        if (account.Status != AccountStatus.Active)
            return false;

        // Check transaction type specific rules
        switch (transactionType)
        {
            case "withdrawal":
            case "transfer":
                return account.HasSufficientBalance(amount);

            case "deposit":
                // Deposits are always allowed for active accounts
                return true;

            default:
                return false;
        }
    }

    public async Task ApplyInterestAsync(
        Account account,
        decimal interestRate,
        CancellationToken cancellationToken = default)
    {
        if (account.Type != AccountType.Savings)
            throw new InvalidOperationException("Interest can only be applied to savings accounts");

        if (account.Status != AccountStatus.Active)
            throw new InvalidOperationException("Interest cannot be applied to inactive accounts");

        if (interestRate <= 0 || interestRate > 1)
            throw new ArgumentException("Interest rate must be between 0 and 1", nameof(interestRate));

        // Calculate interest
        var interestAmount = account.Balance.Amount * interestRate;
        var interest = new Money(Math.Round(interestAmount, 2), account.Balance.Currency);

        // Apply interest as deposit
        account.Deposit(interest, $"Interest applied at {interestRate:P2} rate");

        await _accountRepository.UpdateAsync(account, cancellationToken);
    }

    private async Task<Money> CalculateTransactionFeesAsync(
        Account account,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken)
    {
        // This would calculate transaction fees based on account activity
        // For now, return zero
        return Money.Zero(account.Balance.Currency);
    }
}

/// <summary>
/// Account number generator implementation
/// </summary>
public class AccountNumberGeneratorDomainService : IAccountNumberGeneratorDomainService
{
    private readonly IAccountRepository _accountRepository;

    public AccountNumberGeneratorDomainService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<AccountNumber> GenerateUniqueAccountNumberAsync(
        string accountType,
        CancellationToken cancellationToken = default)
    {
        var maxAttempts = 10;
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            var accountNumber = GenerateAccountNumber(accountType);
            var exists = await _accountRepository.ExistsAsync(accountNumber.Value, cancellationToken);

            if (!exists)
                return accountNumber;

            attempt++;
        }

        throw new InvalidOperationException("Could not generate unique account number");
    }

    private AccountNumber GenerateAccountNumber(string accountType)
    {
        // Generate account number based on type
        var prefix = accountType.ToUpperInvariant() switch
        {
            "CHECKING" => "10",
            "SAVINGS" => "20",
            "INVESTMENT" => "30",
            "CREDIT" => "40",
            _ => "00"
        };

        // Generate 12 random digits
        var random = new Random();
        var suffix = string.Join("", Enumerable.Range(0, 12).Select(_ => random.Next(0, 10)));

        var accountNumberString = $"{prefix}{suffix}";
        return AccountNumber.From(accountNumberString);
    }
}

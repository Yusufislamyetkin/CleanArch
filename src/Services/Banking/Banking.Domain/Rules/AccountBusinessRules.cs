using Enterprise.BuildingBlocks.Domain.Rules;
using Enterprise.Services.Banking.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Rules;

/// <summary>
/// Business rules for Account domain
/// Contains validation logic and business constraints
/// </summary>
public static class AccountBusinessRules
{
    /// <summary>
    /// Rule: Account name cannot be empty or whitespace
    /// </summary>
    public static BusinessRule AccountNameCannotBeEmpty(string accountName)
    {
        return new BusinessRule(
            condition: !string.IsNullOrWhiteSpace(accountName),
            errorMessage: "Account name cannot be empty or whitespace");
    }

    /// <summary>
    /// Rule: Account name must be at least 3 characters long
    /// </summary>
    public static BusinessRule AccountNameMinimumLength(string accountName)
    {
        return new BusinessRule(
            condition: accountName?.Length >= 3,
            errorMessage: "Account name must be at least 3 characters long");
    }

    /// <summary>
    /// Rule: Account name cannot exceed 100 characters
    /// </summary>
    public static BusinessRule AccountNameMaximumLength(string accountName)
    {
        return new BusinessRule(
            condition: accountName?.Length <= 100,
            errorMessage: "Account name cannot exceed 100 characters");
    }

    /// <summary>
    /// Rule: Account type must be valid
    /// </summary>
    public static BusinessRule AccountTypeMustBeValid(string accountType)
    {
        var validTypes = new[] { "Checking", "Savings", "Investment", "Credit" };
        return new BusinessRule(
            condition: validTypes.Contains(accountType),
            errorMessage: $"Account type must be one of: {string.Join(", ", validTypes)}");
    }

    /// <summary>
    /// Rule: Customer cannot have more than specified number of accounts of same type
    /// </summary>
    public static BusinessRule CustomerCannotHaveTooManyAccountsOfType(
        int existingAccountsOfType,
        string accountType,
        int maxAllowed)
    {
        return new BusinessRule(
            condition: existingAccountsOfType < maxAllowed,
            errorMessage: $"Customer cannot have more than {maxAllowed} {accountType} accounts");
    }

    /// <summary>
    /// Rule: Initial balance must meet minimum requirements for account type
    /// </summary>
    public static BusinessRule InitialBalanceMeetsMinimumRequirement(Money initialBalance, string accountType)
    {
        decimal minimumBalance = accountType switch
        {
            "Savings" => 100,
            "Investment" => 1000,
            "Credit" => 0,
            _ => 0
        };

        return new BusinessRule(
            condition: initialBalance.Amount >= minimumBalance,
            errorMessage: $"{accountType} accounts require minimum initial balance of {minimumBalance} {initialBalance.Currency}");
    }

    /// <summary>
    /// Rule: Credit accounts cannot have positive initial balance
    /// </summary>
    public static BusinessRule CreditAccountCannotHavePositiveInitialBalance(Money initialBalance, string accountType)
    {
        return new BusinessRule(
            condition: accountType != "Credit" || initialBalance.Amount == 0,
            errorMessage: "Credit accounts must start with zero balance");
    }

    /// <summary>
    /// Rule: Withdrawal amount cannot be zero or negative
    /// </summary>
    public static BusinessRule WithdrawalAmountMustBePositive(Money amount)
    {
        return new BusinessRule(
            condition: amount.IsPositive,
            errorMessage: "Withdrawal amount must be positive");
    }

    /// <summary>
    /// Rule: Deposit amount cannot be zero or negative
    /// </summary>
    public static BusinessRule DepositAmountMustBePositive(Money amount)
    {
        return new BusinessRule(
            condition: amount.IsPositive,
            errorMessage: "Deposit amount must be positive");
    }

    /// <summary>
    /// Rule: Transfer amount cannot be zero or negative
    /// </summary>
    public static BusinessRule TransferAmountMustBePositive(Money amount)
    {
        return new BusinessRule(
            condition: amount.IsPositive,
            errorMessage: "Transfer amount must be positive");
    }

    /// <summary>
    /// Rule: Account must be active for transactions
    /// </summary>
    public static BusinessRule AccountMustBeActiveForTransactions(AccountStatus status)
    {
        return new BusinessRule(
            condition: status == AccountStatus.Active,
            errorMessage: "Account must be active to perform transactions");
    }

    /// <summary>
    /// Rule: Sufficient balance must be available for withdrawal
    /// </summary>
    public static BusinessRule SufficientBalanceForWithdrawal(Money currentBalance, Money withdrawalAmount, Money? minimumBalance)
    {
        var requiredBalance = withdrawalAmount.Add(minimumBalance ?? Money.Zero(currentBalance.Currency));
        return new BusinessRule(
            condition: currentBalance.IsGreaterThanOrEqual(requiredBalance),
            errorMessage: $"Insufficient balance. Available: {currentBalance}, Required: {requiredBalance}");
    }

    /// <summary>
    /// Rule: Cannot transfer to the same account
    /// </summary>
    public static BusinessRule CannotTransferToSameAccount(Guid fromAccountId, Guid toAccountId)
    {
        return new BusinessRule(
            condition: fromAccountId != toAccountId,
            errorMessage: "Cannot transfer money to the same account");
    }

    /// <summary>
    /// Rule: Transfer between accounts must be same currency
    /// </summary>
    public static BusinessRule TransferCurrenciesMustMatch(string fromCurrency, string toCurrency)
    {
        return new BusinessRule(
            condition: fromCurrency == toCurrency,
            errorMessage: "Cannot transfer between accounts with different currencies");
    }

    /// <summary>
    /// Rule: Daily transaction limit cannot be exceeded
    /// </summary>
    public static BusinessRule DailyTransactionLimitNotExceeded(Money dailyLimit, Money currentDailyTotal, Money transactionAmount)
    {
        if (dailyLimit.IsZero)
            return new BusinessRule(true, string.Empty); // No limit

        return new BusinessRule(
            condition: currentDailyTotal.Add(transactionAmount).IsLessThanOrEqual(dailyLimit),
            errorMessage: $"Daily transaction limit exceeded. Limit: {dailyLimit}, Current: {currentDailyTotal}, Attempted: {transactionAmount}");
    }

    /// <summary>
    /// Rule: Account cannot be closed if it has positive balance
    /// </summary>
    public static BusinessRule AccountCannotBeClosedWithPositiveBalance(Money balance)
    {
        return new BusinessRule(
            condition: !balance.IsPositive,
            errorMessage: "Account cannot be closed while it has a positive balance");
    }
}

/// <summary>
/// Account status enumeration
/// </summary>
public enum AccountStatus
{
    Active = 1,
    Frozen = 2,
    Closed = 3,
    Suspended = 4
}

/// <summary>
/// Account type enumeration
/// </summary>
public enum AccountType
{
    Checking = 1,
    Savings = 2,
    Investment = 3,
    Credit = 4
}

/// <summary>
/// Transaction type enumeration
/// </summary>
public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    TransferIn = 3,
    TransferOut = 4,
    Fee = 5,
    Interest = 6
}

/// <summary>
/// Transaction status enumeration
/// </summary>
public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

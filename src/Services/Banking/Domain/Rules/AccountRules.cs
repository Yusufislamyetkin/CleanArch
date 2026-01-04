using Enterprise.BuildingBlocks.Domain.Rules;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Domain.Rules;

/// <summary>
/// Business rule for account opening validation
/// </summary>
public class AccountOpeningRule : BusinessRule<(Guid CustomerId, AccountType AccountType, Money InitialBalance)>
{
    public AccountOpeningRule((Guid CustomerId, AccountType AccountType, Money InitialBalance) input)
        : base(input)
    {
    }

    public override string RuleName => "AccountOpening";

    public override string Description => "Validates account opening requirements";

    public override bool IsValid()
    {
        var (customerId, accountType, initialBalance) = Input;

        // Customer ID must be valid
        if (customerId == Guid.Empty)
            return false;

        // Initial balance must be non-negative
        if (initialBalance.Amount < 0)
            return false;

        // Account type specific validations
        switch (accountType)
        {
            case AccountType.Savings:
                // Savings accounts require minimum balance
                return initialBalance.Amount >= 1000;

            case AccountType.Investment:
                // Investment accounts require minimum balance
                return initialBalance.Amount >= 5000;

            case AccountType.Credit:
                // Credit accounts must start with zero balance
                return initialBalance.Amount == 0;

            case AccountType.Checking:
            default:
                // Checking accounts can start with any non-negative balance
                return true;
        }
    }

    public override string GetErrorMessage()
    {
        var (customerId, accountType, initialBalance) = Input;

        if (customerId == Guid.Empty)
            return "Customer ID is required";

        if (initialBalance.Amount < 0)
            return "Initial balance cannot be negative";

        switch (accountType)
        {
            case AccountType.Savings:
                return "Savings accounts require minimum 1000 TRY initial balance";

            case AccountType.Investment:
                return "Investment accounts require minimum 5000 TRY initial balance";

            case AccountType.Credit:
                return "Credit accounts must start with zero balance";

            default:
                return "Invalid account opening parameters";
        }
    }
}

/// <summary>
/// Business rule for transaction validation
/// </summary>
public class TransactionValidationRule : BusinessRule<(Account Account, Money Amount, TransactionType Type)>
{
    public TransactionValidationRule((Account Account, Money Amount, TransactionType Type) input)
        : base(input)
    {
    }

    public override string RuleName => "TransactionValidation";

    public override string Description => "Validates transaction requirements";

    public override bool IsValid()
    {
        var (account, amount, type) = Input;

        // Account must be active
        if (account.Status != AccountStatus.Active)
            return false;

        // Amount must be positive
        if (amount.Amount <= 0)
            return false;

        // Currency must match account currency
        if (amount.Currency != account.Balance.Currency)
            return false;

        // Check account type specific rules
        switch (account.Type)
        {
            case AccountType.Credit when type == TransactionType.Withdrawal:
                // Credit accounts cannot have withdrawals
                return false;

            case AccountType.Investment when type == TransactionType.Withdrawal:
                // Investment accounts have withdrawal restrictions
                return amount.Amount <= account.Balance.Amount * 0.1m; // Max 10% withdrawal
        }

        return true;
    }

    public override string GetErrorMessage()
    {
        var (account, amount, type) = Input;

        if (account.Status != AccountStatus.Active)
            return $"Account is not active. Status: {account.Status}";

        if (amount.Amount <= 0)
            return "Transaction amount must be positive";

        if (amount.Currency != account.Balance.Currency)
            return $"Currency mismatch. Account currency: {account.Balance.Currency}, Transaction currency: {amount.Currency}";

        if (account.Type == AccountType.Credit && type == TransactionType.Withdrawal)
            return "Credit accounts do not support withdrawals";

        if (account.Type == AccountType.Investment && type == TransactionType.Withdrawal)
            return "Investment accounts allow maximum 10% withdrawal of current balance";

        return "Invalid transaction parameters";
    }
}

/// <summary>
/// Business rule for daily transaction limits
/// </summary>
public class DailyTransactionLimitRule : BusinessRule<(Account Account, Money TransactionAmount)>
{
    public DailyTransactionLimitRule((Account Account, Money TransactionAmount) input)
        : base(input)
    {
    }

    public override string RuleName => "DailyTransactionLimit";

    public override string Description => "Validates daily transaction limits";

    public override bool IsValid()
    {
        var (account, transactionAmount) = Input;

        // If no daily limit is set, allow transaction
        if (!account.DailyTransactionLimit.HasValue)
            return true;

        // Check if transaction amount exceeds daily limit
        if (transactionAmount.Amount > account.DailyTransactionLimit.Value.Amount)
            return false;

        // Check today's transaction total
        var todayTotal = account.GetTodayTransactionTotal();
        var projectedTotal = todayTotal.Add(transactionAmount);

        return projectedTotal.Amount <= account.DailyTransactionLimit.Value.Amount;
    }

    public override string GetErrorMessage()
    {
        var (account, transactionAmount) = Input;

        if (account.DailyTransactionLimit.HasValue &&
            transactionAmount.Amount > account.DailyTransactionLimit.Value.Amount)
        {
            return $"Transaction amount {transactionAmount.Amount} exceeds daily limit {account.DailyTransactionLimit.Value.Amount}";
        }

        var todayTotal = account.GetTodayTransactionTotal();
        var projectedTotal = todayTotal.Add(transactionAmount);

        return $"Daily limit would be exceeded. Current total: {todayTotal.Amount}, Projected total: {projectedTotal.Amount}, Limit: {account.DailyTransactionLimit?.Amount ?? 0}";
    }
}

/// <summary>
/// Business rule for account closure validation
/// </summary>
public class AccountClosureRule : BusinessRule<Account>
{
    public AccountClosureRule(Account input) : base(input)
    {
    }

    public override string RuleName => "AccountClosure";

    public override string Description => "Validates account closure requirements";

    public override bool IsValid()
    {
        var account = Input;

        // Account must be active
        if (account.Status != AccountStatus.Active)
            return false;

        // Balance must be zero or within acceptable range
        if (account.Balance.Amount < 0)
            return false;

        // For credit accounts, balance must be exactly zero
        if (account.Type == AccountType.Credit && account.Balance.Amount != 0)
            return false;

        // Check minimum balance requirements
        if (account.MinimumBalance.HasValue && account.Balance.Amount < account.MinimumBalance.Value.Amount)
            return false;

        // Check for pending transactions
        // (This would typically query the transaction repository)

        return true;
    }

    public override string GetErrorMessage()
    {
        var account = Input;

        if (account.Status != AccountStatus.Active)
            return $"Cannot close account with status: {account.Status}";

        if (account.Balance.Amount < 0)
            return $"Cannot close account with negative balance: {account.Balance.Amount}";

        if (account.Type == AccountType.Credit && account.Balance.Amount != 0)
            return "Credit accounts must have zero balance to be closed";

        if (account.MinimumBalance.HasValue && account.Balance.Amount < account.MinimumBalance.Value.Amount)
            return $"Account balance {account.Balance.Amount} is below minimum required {account.MinimumBalance.Value.Amount}";

        return "Account cannot be closed due to business rules";
    }
}

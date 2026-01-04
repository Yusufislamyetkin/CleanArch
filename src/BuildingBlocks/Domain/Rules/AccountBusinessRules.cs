using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.BuildingBlocks.Domain.Rules;

/// <summary>
/// Business rule for account creation validation
/// </summary>
public class AccountCreationBusinessRule : BusinessRule<(CustomerId CustomerId, string AccountType, Money InitialBalance)>
{
    public AccountCreationBusinessRule((CustomerId CustomerId, string AccountType, Money InitialBalance) input)
        : base(input)
    {
    }

    public override string RuleName => "AccountCreation";

    public override string Description => "Validates account creation requirements";

    public override bool IsValid()
    {
        var (customerId, accountType, initialBalance) = Input;

        // Customer ID must be valid
        if (customerId.Value == Guid.Empty)
            return false;

        // Initial balance must be non-negative
        if (initialBalance.Amount < 0)
            return false;

        // Account type specific validations
        return accountType.ToLowerInvariant() switch
        {
            "checking" => initialBalance.Amount >= 0,
            "savings" => initialBalance.Amount >= 100, // Minimum 100 TRY for savings
            "investment" => initialBalance.Amount >= 1000, // Minimum 1000 TRY for investment
            "credit" => initialBalance.Amount == 0, // Credit accounts start with 0 balance
            _ => false
        };
    }

    public override string GetErrorMessage()
    {
        var (customerId, accountType, initialBalance) = Input;

        if (customerId.Value == Guid.Empty)
            return "Customer ID is required";

        if (initialBalance.Amount < 0)
            return "Initial balance cannot be negative";

        return accountType.ToLowerInvariant() switch
        {
            "savings" => "Savings accounts require minimum 100 TRY initial balance",
            "investment" => "Investment accounts require minimum 1000 TRY initial balance",
            "credit" => "Credit accounts must start with zero balance",
            _ => "Invalid account type"
        };
    }
}

/// <summary>
/// Business rule for transaction validation
/// </summary>
public class TransactionBusinessRule : BusinessRule<(Guid AccountId, Money Amount, string TransactionType)>
{
    public TransactionBusinessRule((Guid AccountId, Money Amount, string TransactionType) input)
        : base(input)
    {
    }

    public override string RuleName => "Transaction";

    public override string Description => "Validates transaction requirements";

    public override bool IsValid()
    {
        var (accountId, amount, transactionType) = Input;

        // Account ID must be valid
        if (accountId == Guid.Empty)
            return false;

        // Amount must be positive for deposits and withdrawals
        if ((transactionType == "deposit" || transactionType == "withdrawal") && amount.Amount <= 0)
            return false;

        // Amount must be positive for transfers
        if (transactionType == "transfer" && amount.Amount <= 0)
            return false;

        return true;
    }

    public override string GetErrorMessage()
    {
        var (accountId, amount, transactionType) = Input;

        if (accountId == Guid.Empty)
            return "Account ID is required";

        if ((transactionType == "deposit" || transactionType == "withdrawal") && amount.Amount <= 0)
            return "Transaction amount must be positive";

        if (transactionType == "transfer" && amount.Amount <= 0)
            return "Transfer amount must be positive";

        return "Invalid transaction parameters";
    }
}

/// <summary>
/// Business rule for account balance validation
/// </summary>
public class AccountBalanceBusinessRule : BusinessRule<(Money CurrentBalance, Money TransactionAmount, Money? MinimumBalance)>
{
    public AccountBalanceBusinessRule((Money CurrentBalance, Money TransactionAmount, Money? MinimumBalance) input)
        : base(input)
    {
    }

    public override string RuleName => "AccountBalance";

    public override string Description => "Validates account balance after transaction";

    public override bool IsValid()
    {
        var (currentBalance, transactionAmount, minimumBalance) = Input;

        var projectedBalance = currentBalance.Subtract(transactionAmount);
        var requiredMinimum = minimumBalance?.Amount ?? 0;

        return projectedBalance.Amount >= requiredMinimum;
    }

    public override string GetErrorMessage()
    {
        var (currentBalance, transactionAmount, minimumBalance) = Input;

        var projectedBalance = currentBalance.Subtract(transactionAmount);
        var requiredMinimum = minimumBalance?.Amount ?? 0;

        return $"Transaction would result in insufficient balance. Projected balance: {projectedBalance.Amount}, Minimum required: {requiredMinimum}";
    }
}

/// <summary>
/// Business rule for daily transaction limit
/// </summary>
public class DailyTransactionLimitBusinessRule : BusinessRule<(Money DailyTransactionTotal, Money TransactionAmount, Money DailyLimit)>
{
    public DailyTransactionLimitBusinessRule((Money DailyTransactionTotal, Money TransactionAmount, Money DailyLimit) input)
        : base(input)
    {
    }

    public override string RuleName => "DailyTransactionLimit";

    public override string Description => "Validates daily transaction limits";

    public override bool IsValid()
    {
        var (dailyTotal, transactionAmount, dailyLimit) = Input;

        var projectedTotal = dailyTotal.Add(transactionAmount);
        return projectedTotal.Amount <= dailyLimit.Amount;
    }

    public override string GetErrorMessage()
    {
        var (dailyTotal, transactionAmount, dailyLimit) = Input;

        var projectedTotal = dailyTotal.Add(transactionAmount);
        return $"Daily transaction limit would be exceeded. Current total: {dailyTotal.Amount}, Projected total: {projectedTotal.Amount}, Limit: {dailyLimit.Amount}";
    }
}

using Enterprise.BuildingBlocks.Domain.Aggregates;
using Enterprise.Services.Banking.Domain.Events;
using Enterprise.Services.Banking.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Model;

/// <summary>
/// Account aggregate root
/// Represents a bank account with rich domain behavior
/// </summary>
public class Account : AggregateRoot<Guid>
{
    // Private fields for encapsulation
    private readonly List<Transaction> _transactions = new();
    private readonly List<AccountActivity> _activities = new();

    // Public properties (read-only from outside)
    public AccountNumber AccountNumber { get; private set; } = null!;
    public CustomerId CustomerId { get; private set; }
    public string Name { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public AccountStatus Status { get; private set; }
    public Money Balance { get; private set; } = null!;
    public Money? MinimumBalance { get; private set; }
    public Money? DailyTransactionLimit { get; private set; }
    public DateTime? LastTransactionAt { get; private set; }

    // Navigation properties
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    public IReadOnlyCollection<AccountActivity> Activities => _activities.AsReadOnly();

    // Private constructor for EF Core
    private Account() : base() { }

    /// <summary>
    /// Create a new account (factory method)
    /// </summary>
    public static Account Create(
        AccountNumber accountNumber,
        CustomerId customerId,
        string accountName,
        AccountType accountType,
        Money initialBalance,
        Money? minBalance = null,
        Money? dailyTransactionLimit = null)
    {
        // Validate business rules
        if (string.IsNullOrWhiteSpace(accountName))
            throw new ArgumentException("Account name cannot be empty or whitespace");
        if (accountName.Length < 3)
            throw new ArgumentException("Account name must be at least 3 characters long");
        if (accountName.Length > 100)
            throw new ArgumentException("Account name cannot exceed 100 characters");

        var validTypes = new[] { "Checking", "Savings", "Investment", "Credit" };
        if (!validTypes.Contains(accountType.ToString()))
            throw new ArgumentException($"Account type must be one of: {string.Join(", ", validTypes)}");

        decimal minAmount = accountType switch
        {
            AccountType.Savings => 100,
            AccountType.Investment => 1000,
            AccountType.Credit => 0,
            _ => 0
        };

        if (initialBalance.Amount < minAmount)
            throw new ArgumentException($"{accountType} accounts require minimum initial balance of {minAmount} {initialBalance.Currency}");

        if (accountType == AccountType.Credit && initialBalance.Amount != 0)
            throw new ArgumentException("Credit accounts must start with zero balance");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            CustomerId = customerId,
            Name = accountName,
            Type = accountType,
            Status = AccountStatus.Active,
            Balance = initialBalance,
            MinimumBalance = minBalance,
            DailyTransactionLimit = dailyTransactionLimit,
            CreatedAt = DateTime.UtcNow
        };

        // Add domain event
        account.AddDomainEvent(new AccountCreatedDomainEvent(
            account.Id,
            accountNumber,
            customerId,
            accountName,
            accountType.ToString(),
            initialBalance,
            account.CreatedAt));

        return account;
    }

    /// <summary>
    /// Deposit money to the account
    /// </summary>
    public void Deposit(Money amount, string description)
    {
        // Validate business rules
        if (!amount.IsPositive)
            throw new ArgumentException("Deposit amount must be positive");
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Account must be active to perform transactions");

        // Perform deposit
        var newBalance = Balance.Add(amount);
        Balance = newBalance;
        LastTransactionAt = DateTime.UtcNow;

        // Create transaction
        var transaction = Transaction.CreateDeposit(
            Guid.NewGuid(),
            Id,
            amount,
            description,
            DateTime.UtcNow);

        _transactions.Add(transaction);

        // Log activity
        var activity = AccountActivity.Create(
            Guid.NewGuid(),
            Id,
            "Deposit",
            $"Deposited {amount} - {description}",
            DateTime.UtcNow);

        _activities.Add(activity);

        // Add domain event
        AddDomainEvent(new MoneyDepositedDomainEvent(
            Id,
            transaction.Id,
            amount,
            newBalance,
            description,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Withdraw money from the account
    /// </summary>
    public void Withdraw(Money amount, string description)
    {
        // Validate business rules
        if (!amount.IsPositive)
            throw new ArgumentException("Withdrawal amount must be positive");
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Account must be active to perform transactions");

        var availableBalance = GetAvailableBalance();
        var requiredBalance = amount.Add(MinimumBalance ?? Money.Zero(Balance.Currency));
        if (availableBalance.IsLessThan(requiredBalance))
            throw new InvalidOperationException($"Insufficient balance. Available: {availableBalance}, Required: {requiredBalance}");

        // Perform withdrawal
        var newBalance = Balance.Subtract(amount);
        Balance = newBalance;
        LastTransactionAt = DateTime.UtcNow;

        // Create transaction
        var transaction = Transaction.CreateWithdrawal(
            Guid.NewGuid(),
            Id,
            amount,
            description,
            DateTime.UtcNow);

        _transactions.Add(transaction);

        // Log activity
        var activity = AccountActivity.Create(
            Guid.NewGuid(),
            Id,
            "Withdrawal",
            $"Withdrawn {amount} - {description}",
            DateTime.UtcNow);

        _activities.Add(activity);

        // Add domain event
        AddDomainEvent(new MoneyWithdrawnDomainEvent(
            Id,
            transaction.Id,
            amount,
            newBalance,
            description,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Update account name
    /// </summary>
    public void UpdateName(string newName)
    {
        // Validate business rules
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Account name cannot be empty or whitespace");
        if (newName.Length < 3)
            throw new ArgumentException("Account name must be at least 3 characters long");
        if (newName.Length > 100)
            throw new ArgumentException("Account name cannot exceed 100 characters");

        var oldName = Name;
        Name = newName;

        // Log activity
        var activity = AccountActivity.Create(
            Guid.NewGuid(),
            Id,
            "NameUpdate",
            $"Name changed from '{oldName}' to '{newName}'",
            DateTime.UtcNow);

        _activities.Add(activity);

        // Add domain event
        AddDomainEvent(new AccountNameUpdatedDomainEvent(
            Id,
            oldName,
            newName,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Close the account
    /// </summary>
    public void Close()
    {
        // Validate business rules
        if (Balance.IsPositive)
            throw new InvalidOperationException("Account cannot be closed while it has a positive balance");

        Status = AccountStatus.Closed;

        // Log activity
        var activity = AccountActivity.Create(
            Guid.NewGuid(),
            Id,
            "AccountClosed",
            $"Account closed with final balance: {Balance}",
            DateTime.UtcNow);

        _activities.Add(activity);

        // Add domain event
        AddDomainEvent(new AccountClosedDomainEvent(
            Id,
            Balance,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Freeze the account
    /// </summary>
    public void Freeze(string reason)
    {
        Status = AccountStatus.Frozen;

        // Log activity
        var activity = AccountActivity.Create(
            Guid.NewGuid(),
            Id,
            "AccountFrozen",
            $"Account frozen: {reason}",
            DateTime.UtcNow);

        _activities.Add(activity);

        // Add domain event
        AddDomainEvent(new AccountStatusChangedDomainEvent(
            Id,
            AccountStatus.Active.ToString(),
            AccountStatus.Frozen.ToString(),
            reason,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Unfreeze the account
    /// </summary>
    public void Unfreeze()
    {
        Status = AccountStatus.Active;

        // Log activity
        var activity = AccountActivity.Create(
            Guid.NewGuid(),
            Id,
            "AccountUnfrozen",
            "Account unfrozen",
            DateTime.UtcNow);

        _activities.Add(activity);

        // Add domain event
        AddDomainEvent(new AccountStatusChangedDomainEvent(
            Id,
            AccountStatus.Frozen.ToString(),
            AccountStatus.Active.ToString(),
            "Account unfrozen",
            DateTime.UtcNow));
    }

    /// <summary>
    /// Get available balance (considering minimum balance)
    /// </summary>
    public Money GetAvailableBalance()
    {
        return Balance.Subtract(MinimumBalance ?? Money.Zero(Balance.Currency));
    }

    /// <summary>
    /// Check if account has sufficient balance for withdrawal
    /// </summary>
    public bool HasSufficientBalance(Money amount)
    {
        return GetAvailableBalance().IsGreaterThanOrEqual(amount);
    }

    /// <summary>
    /// Get today's transaction total
    /// </summary>
    public Money GetTodayTransactionTotal()
    {
        var today = DateTime.UtcNow.Date;
        var todayTransactions = _transactions
            .Where(t => t.Timestamp.Date == today && t.Status == TransactionStatus.Completed)
            .Sum(t => t.Type == TransactionType.Withdrawal || t.Type == TransactionType.TransferOut
                ? t.Amount.Amount
                : 0);

        return Money.Of(todayTransactions, Balance.Currency);
    }

    /// <summary>
    /// Check if daily transaction limit is exceeded
    /// </summary>
    public bool IsDailyLimitExceeded(Money amount)
    {
        if (DailyTransactionLimit == null || DailyTransactionLimit.IsZero)
            return false;

        return GetTodayTransactionTotal().Add(amount).IsGreaterThan(DailyTransactionLimit);
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

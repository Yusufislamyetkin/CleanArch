using Enterprise.BuildingBlocks.Domain.Aggregates;
using Enterprise.BuildingBlocks.Domain.Entities;
using Enterprise.BuildingBlocks.Domain.Events;
using Enterprise.BuildingBlocks.Domain.Rules;
using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.Model;

/// <summary>
/// Account aggregate root
/// Rich domain model with encapsulated business logic
/// </summary>
public class Account : AggregateRoot<Guid>
{
    private readonly List<Transaction> _transactions = new();
    private Money _balance;
    private AccountStatus _status;
    private readonly List<AccountActivity> _activities = new();

    // Public properties (read-only where appropriate)
    public AccountNumber AccountNumber { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public string Name { get; private set; }
    public AccountType Type { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? LastTransactionAt { get; private set; }
    public Money? MinimumBalance { get; private set; }
    public Money? DailyTransactionLimit { get; private set; }

    // Computed properties
    public Money Balance => _balance;
    public AccountStatus Status => _status;
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    public IReadOnlyCollection<AccountActivity> Activities => _activities.AsReadOnly();

    // Private constructor for EF Core
    private Account() : base()
    {
        Name = string.Empty;
        _balance = Money.Zero("TRY");
        _status = AccountStatus.Inactive;
        Type = AccountType.Checking;
    }

    /// <summary>
    /// Creates a new account with business rule validation
    /// </summary>
    public static Account Create(
        AccountNumber accountNumber,
        CustomerId customerId,
        string name,
        AccountType type,
        Money initialBalance,
        Money? minimumBalance = null,
        Money? dailyTransactionLimit = null)
    {
        // Business rule validation
        var creationRule = new AccountCreationBusinessRule((customerId, type.ToString(), initialBalance));
        creationRule.CheckRule();

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Account name cannot exceed 100 characters", nameof(name));

        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            CustomerId = customerId,
            Name = name.Trim(),
            Type = type,
            _balance = initialBalance,
            _status = AccountStatus.Active,
            OpenedAt = DateTime.UtcNow,
            MinimumBalance = minimumBalance,
            DailyTransactionLimit = dailyTransactionLimit
        };

        // Add domain event
        account.AddDomainEvent(new AccountCreatedDomainEvent(
            account.Id,
            customerId,
            accountNumber,
            type.ToString(),
            initialBalance));

        // Add activity
        account.AddActivity("Account created", initialBalance);

        return account;
    }

    /// <summary>
    /// Deposits money to the account
    /// </summary>
    public void Deposit(Money amount, string description = "")
    {
        EnsureAccountIsActive();
        ValidateTransactionAmount(amount);

        // Check daily limit if applicable
        if (DailyTransactionLimit.HasValue)
        {
            var dailyTotal = GetTodayTransactionTotal();
            var dailyLimitRule = new DailyTransactionLimitBusinessRule(
                (dailyTotal, amount, DailyTransactionLimit.Value));
            dailyLimitRule.CheckRule();
        }

        var previousBalance = _balance;
        _balance = _balance.Add(amount);
        LastTransactionAt = DateTime.UtcNow;

        // Create transaction
        var transaction = Transaction.CreateDeposit(Id, amount, description);
        _transactions.Add(transaction);

        // Add domain event
        AddDomainEvent(new MoneyDepositedDomainEvent(Id, amount, _balance));

        // Add activity
        AddActivity($"Deposit: {description}", amount, previousBalance);

        // Check for balance alerts
        CheckBalanceAlerts();
    }

    /// <summary>
    /// Withdraws money from the account
    /// </summary>
    public void Withdraw(Money amount, string description = "")
    {
        EnsureAccountIsActive();
        ValidateTransactionAmount(amount);

        // Check sufficient balance
        var balanceRule = new AccountBalanceBusinessRule(
            (_balance, amount, MinimumBalance));
        balanceRule.CheckRule();

        // Check daily limit
        if (DailyTransactionLimit.HasValue)
        {
            var dailyTotal = GetTodayTransactionTotal();
            var dailyLimitRule = new DailyTransactionLimitBusinessRule(
                (dailyTotal, amount, DailyTransactionLimit.Value));
            dailyLimitRule.CheckRule();
        }

        var previousBalance = _balance;
        _balance = _balance.Subtract(amount);
        LastTransactionAt = DateTime.UtcNow;

        // Create transaction
        var transaction = Transaction.CreateWithdrawal(Id, amount, description);
        _transactions.Add(transaction);

        // Add domain event
        AddDomainEvent(new MoneyWithdrawnDomainEvent(Id, amount, _balance));

        // Add activity
        AddActivity($"Withdrawal: {description}", amount, previousBalance);

        // Check for balance alerts
        CheckBalanceAlerts();
    }

    /// <summary>
    /// Updates account name
    /// </summary>
    public void UpdateName(string newName)
    {
        EnsureAccountIsActive();

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Account name cannot be empty", nameof(newName));

        if (newName.Length > 100)
            throw new ArgumentException("Account name cannot exceed 100 characters", nameof(newName));

        var oldName = Name;
        Name = newName.Trim();

        // Add activity
        AddActivity($"Name changed from '{oldName}' to '{Name}'", Money.Zero(_balance.Currency));
    }

    /// <summary>
    /// Closes the account
    /// </summary>
    public void Close()
    {
        EnsureAccountIsActive();

        // Business rules for account closure
        if (_balance.Amount > 0)
            throw new InvalidOperationException("Cannot close account with positive balance");

        if (MinimumBalance.HasValue && _balance.Amount < MinimumBalance.Value.Amount)
            throw new InvalidOperationException("Cannot close account with balance below minimum");

        // Check for pending transactions
        var hasPendingTransactions = _transactions.Any(t => t.Status == TransactionStatus.Pending);
        if (hasPendingTransactions)
            throw new InvalidOperationException("Cannot close account with pending transactions");

        _status = AccountStatus.Closed;

        // Add domain event
        AddDomainEvent(new AccountClosedDomainEvent(Id, DateTime.UtcNow));

        // Add activity
        AddActivity("Account closed", _balance);
    }

    /// <summary>
    /// Freezes the account
    /// </summary>
    public void Freeze(string reason)
    {
        EnsureAccountIsActive();

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Freeze reason is required", nameof(reason));

        _status = AccountStatus.Frozen;

        // Add domain event
        AddDomainEvent(new AccountFrozenDomainEvent(Id, reason));

        // Add activity
        AddActivity($"Account frozen: {reason}", _balance);
    }

    /// <summary>
    /// Unfreezes the account
    /// </summary>
    public void Unfreeze()
    {
        if (_status != AccountStatus.Frozen)
            throw new InvalidOperationException("Account is not frozen");

        _status = AccountStatus.Active;

        // Add domain event
        AddDomainEvent(new AccountUnfrozenDomainEvent(Id));

        // Add activity
        AddActivity("Account unfrozen", _balance);
    }

    /// <summary>
    /// Checks if account has sufficient balance for transaction
    /// </summary>
    public bool HasSufficientBalance(Money amount)
    {
        var requiredMinimum = MinimumBalance?.Amount ?? 0;
        return _balance.Amount >= (amount.Amount + requiredMinimum);
    }

    /// <summary>
    /// Gets today's transaction total
    /// </summary>
    public Money GetTodayTransactionTotal()
    {
        var today = DateTime.UtcNow.Date;
        var todayAmount = _transactions
            .Where(t => t.Timestamp.Date == today && t.Status == TransactionStatus.Completed)
            .Sum(t => t.Amount.Amount);

        return new Money(todayAmount, _balance.Currency);
    }

    /// <summary>
    /// Gets available balance (considering minimum balance)
    /// </summary>
    public Money GetAvailableBalance()
    {
        var minimumRequired = MinimumBalance?.Amount ?? 0;
        var availableAmount = Math.Max(0, _balance.Amount - minimumRequired);
        return new Money(availableAmount, _balance.Currency);
    }

    // Private helper methods
    private void EnsureAccountIsActive()
    {
        if (_status != AccountStatus.Active)
            throw new InvalidOperationException($"Account is not active. Current status: {_status}");
    }

    private void ValidateTransactionAmount(Money amount)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(amount));

        if (amount.Currency != _balance.Currency)
            throw new ArgumentException("Currency mismatch", nameof(amount));
    }

    private void AddActivity(string description, Money amount, Money? previousBalance = null)
    {
        var activity = new AccountActivity(
            Guid.NewGuid(),
            Id,
            description,
            amount,
            _balance,
            previousBalance,
            DateTime.UtcNow);

        _activities.Add(activity);
    }

    private void CheckBalanceAlerts()
    {
        // Low balance alert
        if (MinimumBalance.HasValue)
        {
            var threshold = MinimumBalance.Value.Add(new Money(100, _balance.Currency)); // 100 TRY buffer
            if (_balance.Amount <= threshold.Amount && _balance.Amount > MinimumBalance.Value.Amount)
            {
                AddDomainEvent(new AccountBalanceLowDomainEvent(Id, _balance, MinimumBalance.Value));
            }
        }

        // Daily limit exceeded alert
        if (DailyTransactionLimit.HasValue)
        {
            var dailyTotal = GetTodayTransactionTotal();
            if (dailyTotal.Amount >= DailyTransactionLimit.Value.Amount * 0.9m) // 90% of limit
            {
                AddDomainEvent(new DailyLimitExceededDomainEvent(
                    Id, new Money(0, _balance.Currency), DailyTransactionLimit.Value, dailyTotal));
            }
        }
    }
}

/// <summary>
/// Account status enumeration
/// </summary>
public enum AccountStatus
{
    Inactive,
    Active,
    Frozen,
    Closed
}

/// <summary>
/// Account type enumeration
/// </summary>
public enum AccountType
{
    Checking,
    Savings,
    Investment,
    Credit
}
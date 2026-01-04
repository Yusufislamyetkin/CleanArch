using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Application.DTOs;

/// <summary>
/// Account DTO for read operations
/// </summary>
public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public AccountStatus Status { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? MinimumBalance { get; set; }
    public decimal? DailyTransactionLimit { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? LastTransactionAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Account summary DTO
/// </summary>
public class AccountSummaryDto
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public AccountStatus Status { get; set; }
    public decimal? AvailableBalance { get; set; }
    public decimal? MinimumBalance { get; set; }
    public decimal? DailyTransactionLimit { get; set; }
    public Money? TodayTransactionTotal { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? LastTransactionAt { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Account balance DTO
/// </summary>
public class AccountBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? MinimumBalance { get; set; }
    public decimal? DailyTransactionLimit { get; set; }
    public Money? TodayTransactionTotal { get; set; }
    public DateTime AsOf { get; set; }
}

/// <summary>
/// Transaction DTO
/// </summary>
public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ExternalReference { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Account creation request DTO
/// </summary>
public class CreateAccountRequest
{
    public Guid CustomerId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal InitialBalance { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal? MinimumBalance { get; set; }
    public decimal? DailyTransactionLimit { get; set; }
}

/// <summary>
/// Money transfer request DTO
/// </summary>
public class TransferMoneyRequest
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Update account name request DTO
/// </summary>
public class UpdateAccountNameRequest
{
    public string NewName { get; set; } = string.Empty;
}

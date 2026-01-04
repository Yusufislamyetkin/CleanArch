namespace Enterprise.Services.Banking.Api;

/// <summary>
/// Value Objects - Stub implementations
/// </summary>
public record Money(decimal Amount, string Currency);
public record AccountNumber(string Value);
public record CustomerId(Guid Value);

/// <summary>
/// Command DTOs
/// </summary>
public record CreateAccountCommand(
    CustomerId CustomerId,
    string AccountName,
    string AccountType,
    Money InitialBalance,
    Money? MinimumBalance = null,
    Money? DailyTransactionLimit = null) : MediatR.IRequest<CreateAccountResult>;

public record DepositMoneyCommand(
    Guid AccountId,
    Money Amount,
    string Description = "") : MediatR.IRequest<DepositResult>;

public record WithdrawMoneyCommand(
    Guid AccountId,
    Money Amount,
    string Description = "") : MediatR.IRequest<MediatR.Unit>;

public record TransferMoneyCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    string Description = "") : MediatR.IRequest<TransferResult>;

public record UpdateAccountNameCommand(
    Guid AccountId,
    string NewName) : MediatR.IRequest<MediatR.Unit>;

public record CloseAccountCommand(
    Guid AccountId) : MediatR.IRequest<MediatR.Unit>;

/// <summary>
/// Query DTOs
/// </summary>
public record GetAccountByIdQuery(
    Guid AccountId) : MediatR.IRequest<AccountDetailsDto?>;

public record GetAccountByNumberQuery(
    AccountNumber AccountNumber) : MediatR.IRequest<AccountDetailsDto?>;

public record GetCustomerAccountsQuery(
    CustomerId CustomerId,
    bool IncludeClosed = false) : MediatR.IRequest<PaginatedResponse<CustomerAccountSummaryDto>>;

public record SearchAccountsQuery(
    string? SearchTerm = null,
    string? AccountType = null,
    string? Status = null) : MediatR.IRequest<PaginatedResponse<AccountSummaryDto>>;

public record GetAccountTransactionHistoryQuery(
    Guid AccountId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? TransactionType = null) : MediatR.IRequest<PaginatedResponse<TransactionDto>>;

public record GetAccountBalanceQuery(
    Guid AccountId) : MediatR.IRequest<AccountBalanceDto>;

/// <summary>
/// Result DTOs
/// </summary>
public record CreateAccountResult(
    Guid AccountId,
    AccountNumber AccountNumber,
    DateTime CreatedAt);

public record DepositResult(
    Guid AccountId,
    Guid TransactionId,
    Money Amount,
    Money NewBalance);

public record TransferResult(
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    Money Amount,
    Money FromAccountNewBalance,
    Money ToAccountNewBalance);

/// <summary>
/// Response DTOs
/// </summary>
public record AccountDetailsDto(
    Guid Id,
    AccountNumber AccountNumber,
    CustomerId CustomerId,
    string AccountName,
    string AccountType,
    string Status,
    Money CurrentBalance,
    Money AvailableBalance,
    Money? MinimumBalance,
    Money? DailyTransactionLimit,
    Money TodayTransactionTotal,
    DateTime CreatedAt,
    DateTime? LastTransactionAt,
    int TransactionCount);

public record CustomerAccountSummaryDto(
    Guid Id,
    AccountNumber AccountNumber,
    string AccountName,
    string AccountType,
    string Status,
    Money Balance,
    DateTime CreatedAt);

public record AccountSummaryDto(
    Guid Id,
    AccountNumber AccountNumber,
    string AccountName,
    string AccountType,
    string Status,
    Money Balance,
    DateTime CreatedAt);

public record AccountBalanceDto(
    Guid AccountId,
    AccountNumber AccountNumber,
    Money CurrentBalance,
    Money AvailableBalance,
    Money TodayTransactionTotal,
    Money? DailyLimit,
    DateTime AsOf);

public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string Type,
    string Status,
    Money Amount,
    string Description,
    DateTime Timestamp,
    string? ExternalReference);

/// <summary>
/// Pagination DTOs
/// </summary>
public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);

public record PaginationParameters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public void Validate()
    {
        if (PageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0");
        if (PageSize < 1 || PageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100");
    }
}

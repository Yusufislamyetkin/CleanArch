using Enterprise.BuildingBlocks.Application.Queries;
using Enterprise.Services.Banking.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Application.Queries;

/// <summary>
/// Query to get account by ID
/// Read model - optimized for single account retrieval
/// </summary>
public record GetAccountByIdQuery(
    Guid AccountId) : Query<AccountDetailsDto?>;

/// <summary>
/// Query to get account by account number
/// </summary>
public record GetAccountByNumberQuery(
    AccountNumber AccountNumber) : Query<AccountDetailsDto?>;

/// <summary>
/// Query to get accounts by customer ID
/// </summary>
public record GetCustomerAccountsQuery(
    CustomerId CustomerId,
    bool IncludeClosed = false) : Query<PaginatedResponse<CustomerAccountSummaryDto>>;

/// <summary>
/// Query to search accounts
/// </summary>
public record SearchAccountsQuery(
    string? SearchTerm = null,
    string? AccountType = null,
    string? Status = null) : Query<PaginatedResponse<AccountSummaryDto>>;

/// <summary>
/// Query to get account transaction history
/// </summary>
public record GetAccountTransactionHistoryQuery(
    Guid AccountId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? TransactionType = null) : Query<PaginatedResponse<TransactionDto>>;

/// <summary>
/// Query to get account balance
/// </summary>
public record GetAccountBalanceQuery(
    Guid AccountId) : Query<AccountBalanceDto>;

/// <summary>
/// Detailed account DTO - Read model
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

/// <summary>
/// Customer account summary DTO
/// </summary>
public record CustomerAccountSummaryDto(
    Guid Id,
    AccountNumber AccountNumber,
    string AccountName,
    string AccountType,
    string Status,
    Money Balance,
    DateTime CreatedAt);

/// <summary>
/// Account summary DTO for search results
/// </summary>
public record AccountSummaryDto(
    Guid Id,
    AccountNumber AccountNumber,
    string AccountName,
    string AccountType,
    string Status,
    Money Balance,
    DateTime CreatedAt);

/// <summary>
/// Account balance DTO
/// </summary>
public record AccountBalanceDto(
    Guid AccountId,
    AccountNumber AccountNumber,
    Money CurrentBalance,
    Money AvailableBalance,
    Money TodayTransactionTotal,
    Money? DailyLimit,
    DateTime AsOf);

/// <summary>
/// Transaction DTO
/// </summary>
public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string Type,
    string Status,
    Money Amount,
    string Description,
    DateTime Timestamp,
    string? ExternalReference);

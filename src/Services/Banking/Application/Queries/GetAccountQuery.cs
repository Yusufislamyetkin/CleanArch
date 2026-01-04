using Enterprise.BuildingBlocks.Application.Queries;

namespace Enterprise.Services.Banking.Application.Queries;

/// <summary>
/// Query to get account by ID
/// </summary>
public record GetAccountByIdQuery(
    Guid AccountId,
    string? CorrelationId = null,
    string? TenantId = null) : Query<AccountDto?>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Query to get account by account number
/// </summary>
public record GetAccountByNumberQuery(
    string AccountNumber,
    string? CorrelationId = null,
    string? TenantId = null) : Query<AccountDto?>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Query to get accounts by customer ID
/// </summary>
public record GetCustomerAccountsQuery(
    Guid CustomerId,
    bool OnlyActive = true,
    string? CorrelationId = null,
    string? TenantId = null) : Query<IEnumerable<AccountDto>>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Query to search accounts
/// </summary>
public record SearchAccountsQuery(
    string? SearchTerm = null,
    AccountType? AccountType = null,
    AccountStatus? Status = null,
    string? CorrelationId = null,
    string? TenantId = null) : Query<PaginatedResponse<AccountDto>>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
    public override PaginationParameters? Pagination { get; init; } = new() { PageSize = 20 };
}

/// <summary>
/// Query to get account summary
/// </summary>
public record GetAccountSummaryQuery(
    Guid AccountId,
    string? CorrelationId = null,
    string? TenantId = null) : Query<AccountSummaryDto>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

/// <summary>
/// Query to get account transaction history
/// </summary>
public record GetAccountTransactionHistoryQuery(
    Guid AccountId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    TransactionType? TransactionType = null,
    string? CorrelationId = null,
    string? TenantId = null) : Query<PaginatedResponse<TransactionDto>>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
    public override PaginationParameters? Pagination { get; init; } = new() { PageSize = 50 };
}

/// <summary>
/// Query to get account balance
/// </summary>
public record GetAccountBalanceQuery(
    Guid AccountId,
    string? CorrelationId = null,
    string? TenantId = null) : Query<AccountBalanceDto>
{
    public override string? CorrelationId { get; init; } = CorrelationId ?? base.CorrelationId;
    public override string? TenantId { get; init; } = TenantId ?? base.TenantId;
}

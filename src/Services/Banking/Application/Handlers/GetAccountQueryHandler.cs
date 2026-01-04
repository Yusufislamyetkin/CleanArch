using Enterprise.BuildingBlocks.Application.Queries;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Application.Queries;
using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Application.Handlers;

/// <summary>
/// Query handler for getting account by ID
/// </summary>
public class GetAccountByIdQueryHandler : IQueryHandler<GetAccountByIdQuery, AccountDetailsDto?>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByIdQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<AccountDetailsDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            return null;

        return new AccountDetailsDto(
            Id: account.Id,
            AccountNumber: account.AccountNumber,
            CustomerId: account.CustomerId,
            AccountName: account.Name,
            AccountType: account.Type.ToString(),
            Status: account.Status.ToString(),
            CurrentBalance: account.Balance,
            AvailableBalance: account.GetAvailableBalance(),
            MinimumBalance: account.MinimumBalance,
            DailyTransactionLimit: account.DailyTransactionLimit,
            TodayTransactionTotal: account.GetTodayTransactionTotal(),
            CreatedAt: account.CreatedAt,
            LastTransactionAt: account.LastTransactionAt,
            TransactionCount: account.Transactions.Count()
        );
    }
}

/// <summary>
/// Query handler for getting account by account number
/// </summary>
public class GetAccountByNumberQueryHandler : IQueryHandler<GetAccountByNumberQuery, AccountDetailsDto?>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByNumberQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<AccountDetailsDto?> Handle(GetAccountByNumberQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);
        if (account == null)
            return null;

        return new AccountDetailsDto(
            Id: account.Id,
            AccountNumber: account.AccountNumber,
            CustomerId: account.CustomerId,
            AccountName: account.Name,
            AccountType: account.Type.ToString(),
            Status: account.Status.ToString(),
            CurrentBalance: account.Balance,
            AvailableBalance: account.GetAvailableBalance(),
            MinimumBalance: account.MinimumBalance,
            DailyTransactionLimit: account.DailyTransactionLimit,
            TodayTransactionTotal: account.GetTodayTransactionTotal(),
            CreatedAt: account.CreatedAt,
            LastTransactionAt: account.LastTransactionAt,
            TransactionCount: account.Transactions.Count()
        );
    }
}

/// <summary>
/// Query handler for getting customer accounts
/// </summary>
public class GetCustomerAccountsQueryHandler : IQueryHandler<GetCustomerAccountsQuery, PaginatedResponse<CustomerAccountSummaryDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetCustomerAccountsQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<PaginatedResponse<CustomerAccountSummaryDto>> Handle(GetCustomerAccountsQuery request, CancellationToken cancellationToken)
    {
        request.Pagination?.Validate();

        var accounts = await _accountRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (!request.IncludeClosed)
        {
            accounts = accounts.Where(a => a.Status != AccountStatus.Closed);
        }

        var totalCount = accounts.Count();
        var items = accounts
            .OrderByDescending(a => a.CreatedAt)
            .Skip(((request.Pagination?.PageNumber ?? 1) - 1) * (request.Pagination?.PageSize ?? 20))
            .Take(request.Pagination?.PageSize ?? 20)
            .Select(a => new CustomerAccountSummaryDto(
                Id: a.Id,
                AccountNumber: a.AccountNumber,
                AccountName: a.Name,
                AccountType: a.Type.ToString(),
                Status: a.Status.ToString(),
                Balance: a.Balance,
                CreatedAt: a.CreatedAt
            ));

        return new PaginatedResponse<CustomerAccountSummaryDto>
        {
            Items = items,
            PageNumber = request.Pagination?.PageNumber ?? 1,
            PageSize = request.Pagination?.PageSize ?? 20,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Query handler for searching accounts
/// </summary>
public class SearchAccountsQueryHandler : IQueryHandler<SearchAccountsQuery, PaginatedResponse<AccountSummaryDto>>
{
    private readonly IAccountRepository _accountRepository;

    public SearchAccountsQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<PaginatedResponse<AccountSummaryDto>> Handle(SearchAccountsQuery request, CancellationToken cancellationToken)
    {
        request.Pagination?.Validate();

        // This would be more sophisticated in a real implementation
        // For now, get all active accounts
        var accounts = await _accountRepository.GetActiveAccountsAsync(cancellationToken);

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            accounts = accounts.Where(a =>
                a.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.AccountNumber.Value.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrEmpty(request.AccountType) &&
            Enum.TryParse<AccountType>(request.AccountType, true, out var accountType))
        {
            accounts = accounts.Where(a => a.Type == accountType);
        }

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<AccountStatus>(request.Status, true, out var status))
        {
            accounts = accounts.Where(a => a.Status == status);
        }

        var totalCount = accounts.Count();
        var items = accounts
            .OrderByDescending(a => a.CreatedAt)
            .Skip(((request.Pagination?.PageNumber ?? 1) - 1) * (request.Pagination?.PageSize ?? 50))
            .Take(request.Pagination?.PageSize ?? 50)
            .Select(a => new AccountSummaryDto(
                Id: a.Id,
                AccountNumber: a.AccountNumber,
                AccountName: a.Name,
                AccountType: a.Type.ToString(),
                Status: a.Status.ToString(),
                Balance: a.Balance,
                CreatedAt: a.CreatedAt
            ));

        return new PaginatedResponse<AccountSummaryDto>
        {
            Items = items,
            PageNumber = request.Pagination?.PageNumber ?? 1,
            PageSize = request.Pagination?.PageSize ?? 50,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Query handler for getting account transaction history
/// </summary>
public class GetAccountTransactionHistoryQueryHandler : IQueryHandler<GetAccountTransactionHistoryQuery, PaginatedResponse<TransactionDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountTransactionHistoryQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<PaginatedResponse<TransactionDto>> Handle(GetAccountTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        request.Pagination?.Validate();

        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Account {request.AccountId} not found");

        var transactions = account.Transactions.AsQueryable();

        // Apply filters
        if (request.FromDate.HasValue)
            transactions = transactions.Where(t => t.Timestamp >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            transactions = transactions.Where(t => t.Timestamp <= request.ToDate.Value);

        if (!string.IsNullOrEmpty(request.TransactionType) &&
            Enum.TryParse<TransactionType>(request.TransactionType, true, out var transactionType))
        {
            transactions = transactions.Where(t => t.Type == transactionType);
        }

        var totalCount = transactions.Count();
        var items = transactions
            .OrderByDescending(t => t.Timestamp)
            .Skip(((request.Pagination?.PageNumber ?? 1) - 1) * (request.Pagination?.PageSize ?? 25))
            .Take(request.Pagination?.PageSize ?? 25)
            .Select(t => new TransactionDto(
                Id: t.Id,
                AccountId: t.AccountId,
                Type: t.Type.ToString(),
                Status: t.Status.ToString(),
                Amount: t.Amount,
                Description: t.Description,
                Timestamp: t.Timestamp,
                ExternalReference: t.ExternalReference
            ));

        return new PaginatedResponse<TransactionDto>
        {
            Items = items,
            PageNumber = request.Pagination?.PageNumber ?? 1,
            PageSize = request.Pagination?.PageSize ?? 25,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Query handler for getting account balance
/// </summary>
public class GetAccountBalanceQueryHandler : IQueryHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountBalanceQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<AccountBalanceDto> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Account {request.AccountId} not found");

        return new AccountBalanceDto(
            AccountId: account.Id,
            AccountNumber: account.AccountNumber,
            CurrentBalance: account.Balance,
            AvailableBalance: account.GetAvailableBalance(),
            TodayTransactionTotal: account.GetTodayTransactionTotal(),
            DailyLimit: account.DailyTransactionLimit,
            AsOf: DateTime.UtcNow
        );
    }
}

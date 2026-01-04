namespace Enterprise.Services.Banking.Api;

/// <summary>
/// Query handler for getting account by ID - STUB
/// </summary>
public class GetAccountByIdQueryHandler : MediatR.IRequestHandler<GetAccountByIdQuery, AccountDetailsDto?>
{
    public async Task<AccountDetailsDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        return new AccountDetailsDto(
            Id: request.AccountId,
            AccountNumber: new AccountNumber("TEST12345678"),
            CustomerId: new CustomerId(Guid.NewGuid()),
            AccountName: "Test Account",
            AccountType: "Savings",
            Status: "Active",
            CurrentBalance: new Money(1000, "TRY"),
            AvailableBalance: new Money(1000, "TRY"),
            MinimumBalance: new Money(100, "TRY"),
            DailyTransactionLimit: new Money(5000, "TRY"),
            TodayTransactionTotal: new Money(0, "TRY"),
            CreatedAt: DateTime.UtcNow,
            LastTransactionAt: null,
            TransactionCount: 0
        );
    }
}

/// <summary>
/// Query handler for getting account by account number - STUB
/// </summary>
public class GetAccountByNumberQueryHandler : MediatR.IRequestHandler<GetAccountByNumberQuery, AccountDetailsDto?>
{
    public async Task<AccountDetailsDto?> Handle(GetAccountByNumberQuery request, CancellationToken cancellationToken)
    {
        return new AccountDetailsDto(
            Id: Guid.NewGuid(),
            AccountNumber: request.AccountNumber,
            CustomerId: new CustomerId(Guid.NewGuid()),
            AccountName: "Test Account",
            AccountType: "Savings",
            Status: "Active",
            CurrentBalance: new Money(1000, "TRY"),
            AvailableBalance: new Money(1000, "TRY"),
            MinimumBalance: new Money(100, "TRY"),
            DailyTransactionLimit: new Money(5000, "TRY"),
            TodayTransactionTotal: new Money(0, "TRY"),
            CreatedAt: DateTime.UtcNow,
            LastTransactionAt: null,
            TransactionCount: 0
        );
    }
}

/// <summary>
/// Query handler for getting customer accounts - STUB
/// </summary>
public class GetCustomerAccountsQueryHandler : MediatR.IRequestHandler<GetCustomerAccountsQuery, PaginatedResponse<CustomerAccountSummaryDto>>
{
    public async Task<PaginatedResponse<CustomerAccountSummaryDto>> Handle(GetCustomerAccountsQuery request, CancellationToken cancellationToken)
    {
        return new PaginatedResponse<CustomerAccountSummaryDto>
        {
            Items = new[]
            {
                new CustomerAccountSummaryDto(
                    Id: Guid.NewGuid(),
                    AccountNumber: new AccountNumber("TEST12345678"),
                    AccountName: "Test Savings Account",
                    AccountType: "Savings",
                    Status: "Active",
                    Balance: new Money(1000, "TRY"),
                    CreatedAt: DateTime.UtcNow
                )
            },
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 1
        };
    }
}

/// <summary>
/// Query handler for searching accounts - STUB
/// </summary>
public class SearchAccountsQueryHandler : MediatR.IRequestHandler<SearchAccountsQuery, PaginatedResponse<AccountSummaryDto>>
{
    public async Task<PaginatedResponse<AccountSummaryDto>> Handle(SearchAccountsQuery request, CancellationToken cancellationToken)
    {
        return new PaginatedResponse<AccountSummaryDto>
        {
            Items = new[]
            {
                new AccountSummaryDto(
                    Id: Guid.NewGuid(),
                    AccountNumber: new AccountNumber("TEST12345678"),
                    AccountName: "Test Account",
                    AccountType: "Savings",
                    Status: "Active",
                    Balance: new Money(1000, "TRY"),
                    CreatedAt: DateTime.UtcNow
                )
            },
            PageNumber = 1,
            PageSize = 50,
            TotalCount = 1
        };
    }
}

/// <summary>
/// Query handler for getting account transaction history - STUB
/// </summary>
public class GetAccountTransactionHistoryQueryHandler : MediatR.IRequestHandler<GetAccountTransactionHistoryQuery, PaginatedResponse<TransactionDto>>
{
    public async Task<PaginatedResponse<TransactionDto>> Handle(GetAccountTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        return new PaginatedResponse<TransactionDto>
        {
            Items = new[]
            {
                new TransactionDto(
                    Id: Guid.NewGuid(),
                    AccountId: request.AccountId,
                    Type: "Deposit",
                    Status: "Completed",
                    Amount: new Money(100, "TRY"),
                    Description: "Initial deposit",
                    Timestamp: DateTime.UtcNow,
                    ExternalReference: null
                )
            },
            PageNumber = 1,
            PageSize = 25,
            TotalCount = 1
        };
    }
}

/// <summary>
/// Query handler for getting account balance - STUB
/// </summary>
public class GetAccountBalanceQueryHandler : MediatR.IRequestHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    public async Task<AccountBalanceDto> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        return new AccountBalanceDto(
            AccountId: request.AccountId,
            AccountNumber: new AccountNumber("TEST12345678"),
            CurrentBalance: new Money(1000, "TRY"),
            AvailableBalance: new Money(900, "TRY"),
            TodayTransactionTotal: new Money(100, "TRY"),
            DailyLimit: new Money(5000, "TRY"),
            AsOf: DateTime.UtcNow
        );
    }
}
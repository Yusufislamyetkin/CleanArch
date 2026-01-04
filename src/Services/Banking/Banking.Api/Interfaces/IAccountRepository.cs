using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Api.Interfaces;

/// <summary>
/// Account repository interface
/// </summary>
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Account entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    // Domain-specific methods
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work interface
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IAccountRepository GetAccountRepository();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    bool HasActiveTransaction { get; }
}

using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Application.Interfaces;
using Enterprise.Services.Banking.Domain.Model;

namespace Enterprise.Services.Banking.Infrastructure.Persistence;

/// <summary>
/// Account repository implementation
/// Implements domain repository interface using EF Core
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;

    public AccountRepository(BankingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions.OrderByDescending(t => t.Timestamp))
            .Include(a => a.Activities.OrderByDescending(act => act.Timestamp))
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .Include(a => a.Activities)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account> AddAsync(Account entity, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Account entity, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await GetByIdAsync(id, cancellationToken);
        if (account != null)
        {
            _context.Accounts.Remove(account);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == id, cancellationToken);
    }

    // Domain-specific methods
    public async Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions.OrderByDescending(t => t.Timestamp))
            .Include(a => a.Activities.OrderByDescending(act => act.Timestamp))
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .Include(a => a.Activities)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .Include(a => a.Activities)
            .Where(a => a.Status == AccountStatus.Active)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .Include(a => a.Activities)
            .Where(a => a.Type == accountType && a.Status == AccountStatus.Active)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

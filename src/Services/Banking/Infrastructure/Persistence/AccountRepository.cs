using Enterprise.BuildingBlocks.Domain.Entities;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.BuildingBlocks.Infrastructure.Persistence;
using Enterprise.Services.Banking.Domain.Model;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IEnumerable<Account>> GetBySpecificationAsync(
        ISpecification<Account> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Account>> GetPagedAsync(
        ISpecification<Account>? specification = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = specification != null
            ? ApplySpecification(specification)
            : _context.Accounts.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.Transactions)
            .Include(a => a.Activities)
            .ToListAsync(cancellationToken);

        return new PagedResult<Account>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Account> AddAsync(Account entity, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<Account> entities, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddRangeAsync(entities, cancellationToken);
    }

    public async Task UpdateAsync(Account entity, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(entity);
    }

    public async Task UpdateRangeAsync(IEnumerable<Account> entities, CancellationToken cancellationToken = default)
    {
        _context.Accounts.UpdateRange(entities);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await GetByIdAsync(id, cancellationToken);
        if (account != null)
        {
            _context.Accounts.Remove(account);
        }
    }

    public async Task DeleteAsync(Account entity, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Remove(entity);
    }

    public async Task DeleteRangeAsync(IEnumerable<Account> entities, CancellationToken cancellationToken = default)
    {
        _context.Accounts.RemoveRange(entities);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<long> CountAsync(
        ISpecification<Account>? specification = null,
        CancellationToken cancellationToken = default)
    {
        var query = specification != null
            ? ApplySpecification(specification)
            : _context.Accounts.AsQueryable();

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(
        ISpecification<Account>? specification = null,
        CancellationToken cancellationToken = default)
    {
        var query = specification != null
            ? ApplySpecification(specification)
            : _context.Accounts.AsQueryable();

        return await query.AnyAsync(cancellationToken);
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

    private IQueryable<Account> ApplySpecification(ISpecification<Account> specification)
    {
        var query = _context.Accounts.AsQueryable();

        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply then by ordering
        if (specification.ThenBy.Any())
        {
            var orderedQuery = query as IOrderedQueryable<Account>;
            foreach (var thenBy in specification.ThenBy)
            {
                orderedQuery = thenBy.IsDescending
                    ? orderedQuery?.ThenByDescending(thenBy.OrderBy)
                    : orderedQuery?.ThenBy(thenBy.OrderBy);
            }
            query = orderedQuery ?? query;
        }

        // Apply paging
        if (specification.Skip > 0)
        {
            query = query.Skip(specification.Skip);
        }

        if (specification.Take > 0)
        {
            query = query.Take(specification.Take);
        }

        // Apply distinct if needed
        if (specification.IsDistinct)
        {
            query = query.Distinct();
        }

        // Apply tracking
        if (!specification.IsTrackingEnabled)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}

/// <summary>
/// Account repository interface
/// Extends generic repository with domain-specific methods
/// </summary>
public interface IAccountRepository : IRepository<Account, Guid>
{
    Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
}

using Enterprise.Services.Banking.Application.Interfaces;

namespace Enterprise.Services.Banking.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for Banking bounded context
/// Manages transactions and coordinates repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(BankingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(Domain.Model.Account))
        {
            return (IRepository<TEntity, TKey>)new AccountRepository(_context);
        }

        throw new NotSupportedException($"Repository for {typeof(TEntity)} is not implemented");
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle optimistic concurrency conflicts
            throw new InvalidOperationException("Data was modified by another user. Please refresh and try again.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Handle constraint violations and other database errors
            throw new InvalidOperationException("Database operation failed. Please check your data and try again.", ex);
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public bool HasActiveTransaction => _transaction != null;

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }

        await _context.DisposeAsync();
    }
}

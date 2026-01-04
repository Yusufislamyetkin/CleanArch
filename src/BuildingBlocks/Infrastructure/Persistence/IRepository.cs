using Enterprise.BuildingBlocks.Domain.Entities;
using System.Linq.Expressions;

namespace Enterprise.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Generic repository interface
/// Provides basic CRUD operations for all entities
/// </summary>
public interface IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    /// <summary>
    /// Gets entity by ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities with specification
    /// </summary>
    Task<IEnumerable<TEntity>> GetBySpecificationAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paged entities with specification
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(
        ISpecification<TEntity>? specification = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds new entity
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing entity
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes entity by ID
    /// </summary>
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes entity
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if entity exists
    /// </summary>
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities with optional specification
    /// </summary>
    Task<long> CountAsync(
        ISpecification<TEntity>? specification = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specification
    /// </summary>
    Task<bool> AnyAsync(
        ISpecification<TEntity>? specification = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work interface
/// Manages transactions across multiple repositories
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets repository for entity type
    /// </summary>
    IRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
        where TEntity : Entity<TKey>;

    /// <summary>
    /// Saves all changes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if transaction is active
    /// </summary>
    bool HasActiveTransaction { get; }
}

/// <summary>
/// Specification pattern for querying
/// </summary>
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    Expression<Func<T, object>>? GroupBy { get; }
    List<OrderBySpecification<T>> ThenBy { get; }
    int Take { get; }
    int Skip { get; }
    bool IsTrackingEnabled { get; }
    bool IsDistinct { get; }
}

/// <summary>
/// Paged result wrapper
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Order by specification
/// </summary>
public class OrderBySpecification<T>
{
    public Expression<Func<T, object>> OrderBy { get; set; } = null!;
    public bool IsDescending { get; set; }
}
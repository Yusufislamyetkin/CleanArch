namespace Enterprise.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// Cache service interface
/// Provides distributed caching capabilities
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Sets value in cache
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Removes value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets value with factory function
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Sets value with sliding expiration
    /// </summary>
    Task SetSlidingAsync<T>(
        string key,
        T value,
        TimeSpan slidingExpiration,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Refreshes sliding expiration
    /// </summary>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache entry options
/// </summary>
public class CacheEntryOptions
{
    /// <summary>
    /// Absolute expiration time
    /// </summary>
    public DateTimeOffset? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Absolute expiration relative to now
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Sliding expiration
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Cache priority
    /// </summary>
    public CachePriority Priority { get; set; } = CachePriority.Normal;
}

/// <summary>
/// Cache priority levels
/// </summary>
public enum CachePriority
{
    Low,
    Normal,
    High,
    NeverRemove
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// Total number of cache entries
    /// </summary>
    public long EntryCount { get; set; }

    /// <summary>
    /// Cache hit ratio
    /// </summary>
    public double HitRatio { get; set; }

    /// <summary>
    /// Total hits
    /// </summary>
    public long TotalHits { get; set; }

    /// <summary>
    /// Total misses
    /// </summary>
    public long TotalMisses { get; set; }

    /// <summary>
    /// Cache size in bytes
    /// </summary>
    public long SizeBytes { get; set; }
}

/// <summary>
/// Cache key generator
/// Generates standardized cache keys
/// </summary>
public interface ICacheKeyGenerator
{
    /// <summary>
    /// Generates cache key for entity
    /// </summary>
    string GenerateEntityKey<T>(object id);

    /// <summary>
    /// Generates cache key for collection
    /// </summary>
    string GenerateCollectionKey<T>(string? filter = null);

    /// <summary>
    /// Generates cache key for query
    /// </summary>
    string GenerateQueryKey(string queryName, object parameters);
}

/// <summary>
/// Default cache key generator implementation
/// </summary>
public class DefaultCacheKeyGenerator : ICacheKeyGenerator
{
    private const string EntityPrefix = "entity";
    private const string CollectionPrefix = "collection";
    private const string QueryPrefix = "query";

    public string GenerateEntityKey<T>(object id)
    {
        return $"{EntityPrefix}:{typeof(T).Name}:{id}";
    }

    public string GenerateCollectionKey<T>(string? filter = null)
    {
        var key = $"{CollectionPrefix}:{typeof(T).Name}";
        if (!string.IsNullOrEmpty(filter))
        {
            key += $":{filter}";
        }
        return key;
    }

    public string GenerateQueryKey(string queryName, object parameters)
    {
        var paramHash = parameters?.GetHashCode().ToString() ?? "default";
        return $"{QueryPrefix}:{queryName}:{paramHash}";
    }
}

using MediatR;

namespace Enterprise.BuildingBlocks.Application.Queries;

/// <summary>
/// Base query class
/// All queries inherit from this base class
/// </summary>
public abstract record Query<TResponse> : IQuery<TResponse>, IRequest<TResponse>
{
    /// <summary>
    /// Unique identifier for the query
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the query was issued
    /// </summary>
    public DateTime IssuedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Tenant identifier
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// User who issued the query
    /// </summary>
    public string? IssuedBy { get; init; }

    /// <summary>
    /// Query metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Pagination parameters
    /// </summary>
    public PaginationParameters? Pagination { get; init; }
}

/// <summary>
/// Pagination parameters for queries
/// </summary>
public record PaginationParameters
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Maximum allowed page size
    /// </summary>
    public int MaxPageSize { get; init; } = 100;

    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    public void Validate()
    {
        if (PageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0", nameof(PageNumber));

        if (PageSize < 1)
            throw new ArgumentException("Page size must be greater than 0", nameof(PageSize));

        if (PageSize > MaxPageSize)
            throw new ArgumentException($"Page size cannot exceed {MaxPageSize}", nameof(PageSize));
    }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public record PaginatedResponse<T>
{
    /// <summary>
    /// Items in current page
    /// </summary>
    public IEnumerable<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public long TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there are more pages
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there are previous pages
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Marker interface for queries
/// </summary>
public interface IQuery<TResponse>
{
    Guid Id { get; }
    DateTime IssuedAt { get; }
    string? CorrelationId { get; }
    string? TenantId { get; }
    string? IssuedBy { get; }
    Dictionary<string, object>? Metadata { get; }
    PaginationParameters? Pagination { get; }
}

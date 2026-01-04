namespace Enterprise.BuildingBlocks.Domain.Events;

/// <summary>
/// Base class for all domain events
/// Domain events represent business events that have occurred
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Event type name
    /// </summary>
    public string EventType => GetType().Name;

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Tenant identifier
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Event version for schema evolution
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Interface for domain events
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
    string? CorrelationId { get; }
    string? TenantId { get; }
    int Version { get; }
    Dictionary<string, object>? Metadata { get; }
}

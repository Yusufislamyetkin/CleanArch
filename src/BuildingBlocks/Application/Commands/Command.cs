using MediatR;

namespace Enterprise.BuildingBlocks.Application.Commands;

/// <summary>
/// Base command class
/// All commands inherit from this base class
/// </summary>
public abstract record Command : ICommand, IRequest
{
    /// <summary>
    /// Unique identifier for the command
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the command was issued
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
    /// User who issued the command
    /// </summary>
    public string? IssuedBy { get; init; }

    /// <summary>
    /// Command metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Base command with result
/// </summary>
public abstract record Command<TResponse> : ICommand<TResponse>, IRequest<TResponse>
{
    /// <summary>
    /// Unique identifier for the command
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the command was issued
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
    /// User who issued the command
    /// </summary>
    public string? IssuedBy { get; init; }

    /// <summary>
    /// Command metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Marker interface for commands
/// </summary>
public interface ICommand
{
    Guid Id { get; }
    DateTime IssuedAt { get; }
    string? CorrelationId { get; }
    string? TenantId { get; }
    string? IssuedBy { get; }
    Dictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Marker interface for commands with response
/// </summary>
public interface ICommand<TResponse> : ICommand
{
}

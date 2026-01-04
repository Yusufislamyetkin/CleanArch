namespace Enterprise.BuildingBlocks.Domain.Services;

/// <summary>
/// Marker interface for domain services
/// Domain services contain business logic that doesn't naturally fit in entities or value objects
/// </summary>
public interface IDomainService
{
    // Marker interface - no methods required
}

/// <summary>
/// Generic domain service with result
/// </summary>
public interface IDomainService<TInput, TResult>
{
    TResult Execute(TInput input);
}

/// <summary>
/// Async domain service
/// </summary>
public interface IAsyncDomainService<TInput, TResult>
{
    Task<TResult> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

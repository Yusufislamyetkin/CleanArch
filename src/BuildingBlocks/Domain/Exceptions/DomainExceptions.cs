namespace Enterprise.BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Base exception for domain layer
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Error code for categorization
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    public string? CorrelationId { get; }

    protected DomainException(string message, string errorCode, string? correlationId = null)
        : base(message)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
    }

    protected DomainException(string message, string errorCode, Exception innerException, string? correlationId = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        CorrelationId = correlationId;
    }
}

/// <summary>
/// Business rule violation exception
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message, string? correlationId = null)
        : base(message, "BUSINESS_RULE_VIOLATION", correlationId)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Aggregate not found exception
/// </summary>
public class AggregateNotFoundException : DomainException
{
    public string AggregateType { get; }
    public object AggregateId { get; }

    public AggregateNotFoundException(string aggregateType, object aggregateId, string? correlationId = null)
        : base($"{aggregateType} with ID '{aggregateId}' not found", "AGGREGATE_NOT_FOUND", correlationId)
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
    }
}

/// <summary>
/// Invalid aggregate state exception
/// </summary>
public class InvalidAggregateStateException : DomainException
{
    public string AggregateType { get; }
    public object AggregateId { get; }

    public InvalidAggregateStateException(string aggregateType, object aggregateId, string message, string? correlationId = null)
        : base(message, "INVALID_AGGREGATE_STATE", correlationId)
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
    }
}

/// <summary>
/// Concurrency conflict exception
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    public string AggregateType { get; }
    public object AggregateId { get; }
    public long ExpectedVersion { get; }
    public long ActualVersion { get; }

    public ConcurrencyConflictException(
        string aggregateType,
        object aggregateId,
        long expectedVersion,
        long actualVersion,
        string? correlationId = null)
        : base($"Concurrency conflict in {aggregateType} with ID '{aggregateId}'. Expected version: {expectedVersion}, Actual version: {actualVersion}",
              "CONCURRENCY_CONFLICT", correlationId)
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}

/// <summary>
/// Domain validation exception
/// </summary>
public class DomainValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }

    public DomainValidationException(Dictionary<string, string[]> errors, string? correlationId = null)
        : base("Domain validation failed", "DOMAIN_VALIDATION_FAILED", correlationId)
    {
        Errors = errors;
    }

    public DomainValidationException(string field, string error, string? correlationId = null)
        : this(new Dictionary<string, string[]> { [field] = new[] { error } }, correlationId)
    {
    }
}

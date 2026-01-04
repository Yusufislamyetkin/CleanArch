namespace Enterprise.BuildingBlocks.Infrastructure.Monitoring;

/// <summary>
/// Monitoring service interface
/// Provides application metrics and health monitoring
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// Records a counter metric
    /// </summary>
    void IncrementCounter(string name, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Records a gauge metric
    /// </summary>
    void SetGauge(string name, double value, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Records a histogram metric
    /// </summary>
    void RecordHistogram(string name, double value, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Records a timer metric
    /// </summary>
    IDisposable StartTimer(string name, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Records an event
    /// </summary>
    void RecordEvent(string name, Dictionary<string, object>? properties = null);

    /// <summary>
    /// Gets current metrics snapshot
    /// </summary>
    Task<MetricsSnapshot> GetMetricsSnapshotAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Health check service interface
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs health check
    /// </summary>
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a health check
    /// </summary>
    void RegisterHealthCheck(string name, Func<CancellationToken, Task<HealthCheckResult>> check);

    /// <summary>
    /// Unregisters a health check
    /// </summary>
    void UnregisterHealthCheck(string name);
}

/// <summary>
/// Tracing service interface
/// Provides distributed tracing capabilities
/// </summary>
public interface ITracingService
{
    /// <summary>
    /// Starts a new trace span
    /// </summary>
    ISpan StartSpan(string operationName, SpanKind kind = SpanKind.Internal);

    /// <summary>
    /// Gets current span
    /// </summary>
    ISpan? CurrentSpan { get; }

    /// <summary>
    /// Extracts span context from carrier
    /// </summary>
    SpanContext? Extract<TCarrier>(TCarrier carrier, string format);

    /// <summary>
    /// Injects span context into carrier
    /// </summary>
    void Inject<TCarrier>(SpanContext spanContext, TCarrier carrier, string format);
}

/// <summary>
/// Span interface for tracing
/// </summary>
public interface ISpan : IDisposable
{
    /// <summary>
    /// Span context
    /// </summary>
    SpanContext Context { get; }

    /// <summary>
    /// Sets tag
    /// </summary>
    ISpan SetTag(string key, string value);

    /// <summary>
    /// Sets tag with boolean value
    /// </summary>
    ISpan SetTag(string key, bool value);

    /// <summary>
    /// Sets tag with numeric value
    /// </summary>
    ISpan SetTag(string key, double value);

    /// <summary>
    /// Logs an event
    /// </summary>
    ISpan LogEvent(string eventName);

    /// <summary>
    /// Logs an exception
    /// </summary>
    ISpan LogException(Exception exception);

    /// <summary>
    /// Finishes the span
    /// </summary>
    void Finish();
}

/// <summary>
/// Span context
/// </summary>
public class SpanContext
{
    /// <summary>
    /// Trace ID
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Span ID
    /// </summary>
    public string SpanId { get; set; } = string.Empty;

    /// <summary>
    /// Parent span ID
    /// </summary>
    public string? ParentSpanId { get; set; }

    /// <summary>
    /// Service name
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Operation name
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Span kind
    /// </summary>
    public SpanKind Kind { get; set; }

    /// <summary>
    /// Start time
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Tags
    /// </summary>
    public Dictionary<string, object> Tags { get; set; } = new();
}

/// <summary>
/// Span kind
/// </summary>
public enum SpanKind
{
    Internal,
    Server,
    Client,
    Producer,
    Consumer
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Health status
    /// </summary>
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Duration
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Exception if any
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Additional data
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Individual check results
    /// </summary>
    public Dictionary<string, HealthCheckResult>? Entries { get; set; }
}

/// <summary>
/// Health status
/// </summary>
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>
/// Metrics snapshot
/// </summary>
public class MetricsSnapshot
{
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Counters
    /// </summary>
    public Dictionary<string, long> Counters { get; set; } = new();

    /// <summary>
    /// Gauges
    /// </summary>
    public Dictionary<string, double> Gauges { get; set; } = new();

    /// <summary>
    /// Histograms
    /// </summary>
    public Dictionary<string, HistogramData> Histograms { get; set; } = new();

    /// <summary>
    /// Timers
    /// </summary>
    public Dictionary<string, TimerData> Timers { get; set; } = new();
}

/// <summary>
/// Histogram data
/// </summary>
public class HistogramData
{
    /// <summary>
    /// Count
    /// </summary>
    public long Count { get; set; }

    /// <summary>
    /// Sum
    /// </summary>
    public double Sum { get; set; }

    /// <summary>
    /// Mean
    /// </summary>
    public double Mean { get; set; }

    /// <summary>
    /// Min
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Max
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Percentiles
    /// </summary>
    public Dictionary<double, double> Percentiles { get; set; } = new();
}

/// <summary>
/// Timer data
/// </summary>
public class TimerData
{
    /// <summary>
    /// Count
    /// </summary>
    public long Count { get; set; }

    /// <summary>
    /// Total time
    /// </summary>
    public TimeSpan TotalTime { get; set; }

    /// <summary>
    /// Mean time
    /// </summary>
    public TimeSpan MeanTime { get; set; }

    /// <summary>
    /// Min time
    /// </summary>
    public TimeSpan MinTime { get; set; }

    /// <summary>
    /// Max time
    /// </summary>
    public TimeSpan MaxTime { get; set; }
}

using Enterprise.BuildingBlocks.Application.Commands;
using Enterprise.BuildingBlocks.Application.Queries;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enterprise.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Validation behavior for commands and queries
/// Validates requests using FluentValidation before processing
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators ?? Array.Empty<IValidator<TRequest>>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get validators for this request type
        var validators = _validators.ToArray();
        if (!validators.Any())
        {
            return await next();
        }

        // Validate the request
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToArray();

        if (failures.Any())
        {
            _logger.LogWarning("Validation failed for {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

            throw new ValidationException(failures);
        }

        return await next();
    }
}

/// <summary>
/// Logging behavior for all requests
/// Logs request execution time and details
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = GetRequestId(request);

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["RequestType"] = requestName,
            ["CorrelationId"] = GetCorrelationId(request) ?? string.Empty
        }))
        {
            _logger.LogInformation("Processing {RequestType} with ID {RequestId}", requestName, requestId);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var response = await next();
                stopwatch.Stop();

                _logger.LogInformation("Successfully processed {RequestType} in {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex, "Failed to process {RequestType} after {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }

    private static string GetRequestId(TRequest request)
    {
        if (request is IQuery<TResponse> query)
            return query.Id.ToString();
        if (request is ICommand command)
            return command.Id.ToString();
        return Guid.NewGuid().ToString();
    }

    private static string? GetCorrelationId(TRequest request)
    {
        if (request is IQuery<TResponse> query)
            return query.CorrelationId;
        if (request is ICommand command)
            return command.CorrelationId;
        return null;
    }
}

/// <summary>
/// Performance monitoring behavior
/// Tracks slow requests and performance metrics
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int SlowRequestThresholdMs = 1000; // 1 second

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log slow requests
            if (elapsedMs > SlowRequestThresholdMs)
            {
                _logger.LogWarning("Slow request detected: {RequestType} took {ElapsedMilliseconds}ms",
                    requestName, elapsedMs);
            }
            else
            {
                _logger.LogDebug("Request {RequestType} completed in {ElapsedMilliseconds}ms",
                    requestName, elapsedMs);
            }

            // Here you could send metrics to monitoring systems
            // await _metricsService.RecordRequestDuration(requestName, elapsedMs);

            return response;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            throw;
        }
    }
}

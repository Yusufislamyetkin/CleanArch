namespace Enterprise.Services.Banking.Api.Middleware;

/// <summary>
/// Request logging middleware
/// Logs incoming requests and outgoing responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                          ?? Guid.NewGuid().ToString();

        // Add correlation ID to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var request = context.Request;
        var startTime = DateTime.UtcNow;

        // Log incoming request
        _logger.LogInformation(
            "Request started. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, Query: {Query}, UserAgent: {UserAgent}, RemoteIp: {RemoteIp}",
            correlationId,
            request.Method,
            request.Path,
            request.QueryString,
            request.Headers["User-Agent"].ToString(),
            context.Connection.RemoteIpAddress?.ToString());

        try
        {
            await _next(context);

            var duration = DateTime.UtcNow - startTime;

            // Log successful response
            _logger.LogInformation(
                "Request completed successfully. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                correlationId,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            // Log failed request
            _logger.LogWarning(ex,
                "Request failed. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                correlationId,
                context.Response.StatusCode,
                duration.TotalMilliseconds);

            throw;
        }
    }
}

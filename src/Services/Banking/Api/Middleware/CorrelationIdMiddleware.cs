namespace Enterprise.Services.Banking.Api.Middleware;

/// <summary>
/// Correlation ID middleware
/// Ensures every request has a correlation ID for tracing
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Add correlation ID to request context for use in controllers/services
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        await _next(context);
    }
}

/// <summary>
/// Extension methods for correlation ID
/// </summary>
public static class CorrelationIdExtensions
{
    private const string CorrelationIdKey = "CorrelationId";

    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items[CorrelationIdKey] as string;
    }

    public static void SetCorrelationId(this HttpContext context, string correlationId)
    {
        context.Items[CorrelationIdKey] = correlationId;
    }
}

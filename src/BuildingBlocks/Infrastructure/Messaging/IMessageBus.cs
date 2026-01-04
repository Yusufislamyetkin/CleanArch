namespace Enterprise.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Message bus interface for publish/subscribe pattern
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publishes message to exchange/routing key
    /// </summary>
    Task PublishAsync<TMessage>(
        TMessage message,
        string routingKey,
        MessageProperties? properties = null,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Publishes message to specific exchange
    /// </summary>
    Task PublishToExchangeAsync<TMessage>(
        string exchange,
        string routingKey,
        TMessage message,
        MessageProperties? properties = null,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Sends message to specific queue
    /// </summary>
    Task SendToQueueAsync<TMessage>(
        string queueName,
        TMessage message,
        MessageProperties? properties = null,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Subscribes to messages with routing key pattern
    /// </summary>
    Task SubscribeAsync<TMessage>(
        string routingKey,
        Func<TMessage, MessageProperties, Task> handler,
        string? queueName = null,
        CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Creates a consumer for specific queue
    /// </summary>
    IMessageConsumer CreateConsumer(string queueName);
}

/// <summary>
/// Message consumer interface
/// </summary>
public interface IMessageConsumer : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Starts consuming messages
    /// </summary>
    Task StartAsync(Func<MessageEnvelope, Task> handler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops consuming messages
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Message envelope wrapper
/// </summary>
public class MessageEnvelope
{
    /// <summary>
    /// Message body
    /// </summary>
    public object Body { get; set; } = null!;

    /// <summary>
    /// Message properties
    /// </summary>
    public MessageProperties Properties { get; set; } = null!;

    /// <summary>
    /// Delivery tag for acknowledging
    /// </summary>
    public ulong DeliveryTag { get; set; }

    /// <summary>
    /// Consumer tag
    /// </summary>
    public string ConsumerTag { get; set; } = string.Empty;

    /// <summary>
    /// Routing key
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
}

/// <summary>
/// Message properties
/// </summary>
public class MessageProperties
{
    /// <summary>
    /// Message ID
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Correlation ID for request/response correlation
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User ID who sent the message
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Application ID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// Message priority
    /// </summary>
    public byte? Priority { get; set; }

    /// <summary>
    /// Message timestamp
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Content type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Content encoding
    /// </summary>
    public string? ContentEncoding { get; set; }

    /// <summary>
    /// Message expiration
    /// </summary>
    public string? Expiration { get; set; }

    /// <summary>
    /// Custom headers
    /// </summary>
    public Dictionary<string, object>? Headers { get; set; }

    /// <summary>
    /// Delivery mode (persistent or not)
    /// </summary>
    public bool Persistent { get; set; } = true;

    /// <summary>
    /// Tenant identifier for multi-tenancy
    /// </summary>
    public string? TenantId { get; set; }
}

/// <summary>
/// Message bus configuration
/// </summary>
public class MessageBusConfiguration
{
    /// <summary>
    /// Host name
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Client name
    /// </summary>
    public string ClientName { get; set; } = "Enterprise.Banking";

    /// <summary>
    /// Connection retry count
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Connection retry delay
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Publisher confirms enabled
    /// </summary>
    public bool PublisherConfirmsEnabled { get; set; } = true;

    /// <summary>
    /// Consumer dispatch concurrency
    /// </summary>
    public int ConsumerDispatchConcurrency { get; set; } = 1;
}

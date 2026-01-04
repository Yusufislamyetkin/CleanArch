using Enterprise.BuildingBlocks.Domain.Entities;
using Enterprise.BuildingBlocks.Domain.Events;

namespace Enterprise.BuildingBlocks.Domain.Aggregates;

/// <summary>
/// Base class for aggregate roots
/// Manages domain events and enforces aggregate boundaries
/// </summary>
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Uncommitted domain events
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Protected constructor
    /// </summary>
    protected AggregateRoot() : base()
    {
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    protected AggregateRoot(TKey id) : base(id)
    {
    }

    /// <summary>
    /// Adds a domain event to the aggregate
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events after they are processed
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Applies a domain event to the aggregate
    /// </summary>
    protected virtual void Apply(IDomainEvent @event)
    {
        // Override in derived classes to apply events
    }

    /// <summary>
    /// Loads aggregate from event history
    /// </summary>
    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var @event in history)
        {
            Apply(@event);
            Version++;
        }
    }
}

/// <summary>
/// Marker interface for aggregate roots
/// </summary>
public interface IAggregateRoot<TKey> : IEntity<TKey>
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

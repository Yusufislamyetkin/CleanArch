using System.ComponentModel.DataAnnotations.Schema;

namespace Enterprise.BuildingBlocks.Domain.Entities;

/// <summary>
/// Base entity class for all domain entities
/// Implements common entity properties and behaviors
/// </summary>
public abstract class Entity<TKey> : IEntity<TKey>
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public TKey Id { get; protected set; } = default!;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Version for optimistic concurrency
    /// </summary>
    public long Version { get; protected set; }

    /// <summary>
    /// Tenant identifier for multi-tenancy
    /// </summary>
    public string? TenantId { get; protected set; }

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    [NotMapped]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
        Version = 1;
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    protected Entity(TKey id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        Version = 1;
    }

    /// <summary>
    /// Marks entity as modified
    /// </summary>
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Soft delete the entity
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        MarkAsModified();
    }

    /// <summary>
    /// Restore the entity from soft delete
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        MarkAsModified();
    }

    /// <summary>
    /// Set tenant for multi-tenancy
    /// </summary>
    public void SetTenant(string tenantId)
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Equality based on ID
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id != null && Id.Equals(other.Id);
    }

    /// <summary>
    /// Hash code based on ID
    /// </summary>
    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Marker interface for entities
/// </summary>
public interface IEntity<TKey>
{
    TKey Id { get; }
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    bool IsDeleted { get; }
    long Version { get; }
    string? TenantId { get; }
}

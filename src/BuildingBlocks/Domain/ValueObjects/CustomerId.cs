namespace Enterprise.BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// Customer ID value object
/// Strongly typed identifier for customers
/// </summary>
public sealed class CustomerId : IEquatable<CustomerId>
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(value));

        Value = value;
    }

    public static CustomerId From(Guid value) => new(value);
    public static CustomerId NewId() => new(Guid.NewGuid());

    public override bool Equals(object? obj) => Equals(obj as CustomerId);

    public bool Equals(CustomerId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
    public static explicit operator CustomerId(Guid value) => From(value);

    public static bool operator ==(CustomerId? left, CustomerId? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(CustomerId? left, CustomerId? right) => !(left == right);
}

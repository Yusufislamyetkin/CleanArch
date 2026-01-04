namespace Enterprise.BuildingBlocks.Domain.ValueObjects;

/// <summary>
/// Base class for value objects
/// Value objects are immutable and compared by value, not reference
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the atomic values that define the value object
    /// </summary>
    protected abstract IEnumerable<object> GetAtomicValues();

    /// <summary>
    /// Value equality comparison
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        using var thisValues = GetAtomicValues().GetEnumerator();
        using var otherValues = other.GetAtomicValues().GetEnumerator();

        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
            if (thisValues.Current is null ^ otherValues.Current is null)
                return false;

            if (thisValues.Current != null && !thisValues.Current.Equals(otherValues.Current))
                return false;
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
    }

    /// <summary>
    /// Hash code based on atomic values
    /// </summary>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Generic value object with validation
/// </summary>
public abstract class ValueObject<T> : ValueObject where T : ValueObject<T>
{
    /// <summary>
    /// Validates the value object
    /// </summary>
    protected abstract void Validate();

    /// <summary>
    /// Creates a new value object with validation
    /// </summary>
    protected static T Create(T value)
    {
        value.Validate();
        return value;
    }
}

using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.ValueObjects;

/// <summary>
/// Customer identifier value object
/// Represents unique customer identifiers
/// </summary>
public sealed class CustomerId : ValueObject
{
    public Guid Value { get; private set; }

    private CustomerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Factory method to create CustomerId from Guid
    /// </summary>
    public static CustomerId From(Guid value)
    {
        return new CustomerId(value);
    }

    /// <summary>
    /// Factory method to create CustomerId from string
    /// </summary>
    public static CustomerId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException("Invalid GUID format", nameof(value));

        return new CustomerId(guid);
    }

    /// <summary>
    /// Create new CustomerId
    /// </summary>
    public static CustomerId NewId()
    {
        return new CustomerId(Guid.NewGuid());
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    // Implicit conversion operators
    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
    public static implicit operator CustomerId(Guid value) => new CustomerId(value);
}

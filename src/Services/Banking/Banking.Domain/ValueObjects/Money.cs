using Enterprise.BuildingBlocks.Domain.ValueObjects;

namespace Enterprise.Services.Banking.Domain.ValueObjects;

/// <summary>
/// Money value object - represents monetary amounts with currency
/// Implements Value Object pattern for immutability and equality
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Factory method to create Money instance
    /// </summary>
    public static Money Of(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Zero amount in specified currency
    /// </summary>
    public static Money Zero(string currency)
    {
        return new Money(0, currency);
    }

    /// <summary>
    /// Add two Money instances (must be same currency)
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtract Money from this instance (must be same currency)
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(result, Currency);
    }

    /// <summary>
    /// Multiply by a factor
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Check if this money is greater than other
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return Amount > other.Amount;
    }

    /// <summary>
    /// Check if this money is greater than or equal to other
    /// </summary>
    public bool IsGreaterThanOrEqual(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return Amount >= other.Amount;
    }

    /// <summary>
    /// Check if this money is less than other
    /// </summary>
    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return Amount < other.Amount;
    }

    /// <summary>
    /// Check if this money is less than or equal to other
    /// </summary>
    public bool IsLessThanOrEqual(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return Amount <= other.Amount;
    }

    /// <summary>
    /// Check if amount is zero
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Check if amount is positive
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Check if amount is negative
    /// </summary>
    public bool IsNegative => Amount < 0;

    /// <summary>
    /// Get absolute value
    /// </summary>
    public Money Absolute => new Money(Math.Abs(Amount), Currency);

    /// <summary>
    /// Get negated value
    /// </summary>
    public Money Negate => new Money(-Amount, Currency);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount:N2} {Currency}";
    }

    // Operator overloads for convenience
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static bool operator >(Money left, Money right) => left.IsGreaterThan(right);
    public static bool operator >=(Money left, Money right) => left.IsGreaterThanOrEqual(right);
    public static bool operator <(Money left, Money right) => left.IsLessThan(right);
    public static bool operator <=(Money left, Money right) => left.IsLessThanOrEqual(right);
}

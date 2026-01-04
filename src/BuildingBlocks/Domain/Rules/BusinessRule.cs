using Enterprise.BuildingBlocks.Domain.Exceptions;

namespace Enterprise.BuildingBlocks.Domain.Rules;

/// <summary>
/// Base class for business rules
/// Encapsulates business logic validation
/// </summary>
public abstract class BusinessRule
{
    /// <summary>
    /// Rule name for identification
    /// </summary>
    public abstract string RuleName { get; }

    /// <summary>
    /// Human-readable description
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Validates the rule
    /// </summary>
    public abstract bool IsValid();

    /// <summary>
    /// Gets the error message if rule is violated
    /// </summary>
    public abstract string GetErrorMessage();

    /// <summary>
    /// Checks the rule and throws exception if invalid
    /// </summary>
    public void CheckRule()
    {
        if (!IsValid())
        {
            throw new BusinessRuleViolationException(RuleName, GetErrorMessage());
        }
    }

    /// <summary>
    /// Async version of rule checking
    /// </summary>
    public async Task CheckRuleAsync()
    {
        if (!await IsValidAsync())
        {
            throw new BusinessRuleViolationException(RuleName, GetErrorMessage());
        }
    }

    /// <summary>
    /// Async validation (default implementation calls sync version)
    /// </summary>
    protected virtual Task<bool> IsValidAsync()
    {
        return Task.FromResult(IsValid());
    }
}

/// <summary>
/// Generic business rule with input
/// </summary>
public abstract class BusinessRule<TInput> : BusinessRule
{
    /// <summary>
    /// Input for rule validation
    /// </summary>
    protected TInput Input { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    protected BusinessRule(TInput input)
    {
        Input = input;
    }
}

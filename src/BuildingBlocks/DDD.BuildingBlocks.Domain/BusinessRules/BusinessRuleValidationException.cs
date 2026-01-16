namespace BuildingBlocks.Domain.BusinessRules;

/// <summary>
/// Exception thrown when a business rule is violated.
/// Contains information about the specific rule that was broken.
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    /// <summary>
    /// The business rule that was broken.
    /// </summary>
    public IBusinessRule BrokenRule { get; }

    /// <summary>
    /// Additional details about the rule violation for debugging purposes.
    /// </summary>
    public string Details { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Code, brokenRule.Message)
    {
        BrokenRule = brokenRule;
        Details = $"Business rule '{brokenRule.GetType().Name}' was violated.";
    }

    public BusinessRuleValidationException(IBusinessRule brokenRule, string details)
        : base(brokenRule.Code, brokenRule.Message)
    {
        BrokenRule = brokenRule;
        Details = details;
    }

    public override string ToString()
    {
        return $"{Details} Message: '{Message}'";
    }
}

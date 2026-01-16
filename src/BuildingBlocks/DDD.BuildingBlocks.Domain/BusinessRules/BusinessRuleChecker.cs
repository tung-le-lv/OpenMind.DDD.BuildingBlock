namespace BuildingBlocks.Domain.BusinessRules;

/// <summary>
/// Static helper for checking business rules outside of entities.
/// Useful in domain services or application layer validation.
/// </summary>
public static class BusinessRuleChecker
{
    /// <summary>
    /// Checks a single business rule and throws if broken.
    /// </summary>
    /// <param name="rule">The rule to check.</param>
    /// <exception cref="BusinessRuleValidationException">Thrown when the rule is broken.</exception>
    public static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }

    /// <summary>
    /// Checks multiple business rules and throws on the first broken rule.
    /// </summary>
    /// <param name="rules">The rules to check.</param>
    /// <exception cref="BusinessRuleValidationException">Thrown when any rule is broken.</exception>
    public static void CheckRules(params IBusinessRule[] rules)
    {
        foreach (var rule in rules)
        {
            CheckRule(rule);
        }
    }

    /// <summary>
    /// Checks multiple business rules and returns all broken rules.
    /// Does not throw an exception.
    /// </summary>
    /// <param name="rules">The rules to check.</param>
    /// <returns>Collection of broken rules.</returns>
    public static IReadOnlyList<IBusinessRule> GetBrokenRules(params IBusinessRule[] rules)
    {
        return rules.Where(r => r.IsBroken()).ToList();
    }

    /// <summary>
    /// Validates all rules and throws an aggregate exception if any are broken.
    /// </summary>
    /// <param name="rules">The rules to validate.</param>
    /// <exception cref="AggregateBusinessRuleValidationException">Thrown when any rules are broken.</exception>
    public static void ValidateAll(params IBusinessRule[] rules)
    {
        var brokenRules = GetBrokenRules(rules);
        if (brokenRules.Count > 0)
        {
            throw new AggregateBusinessRuleValidationException(brokenRules);
        }
    }
}

/// <summary>
/// Exception thrown when multiple business rules are violated.
/// </summary>
public class AggregateBusinessRuleValidationException : DomainException
{
    public IReadOnlyList<IBusinessRule> BrokenRules { get; }

    public AggregateBusinessRuleValidationException(IReadOnlyList<IBusinessRule> brokenRules)
        : base("MULTIPLE_RULES_VIOLATED", 
               string.Join("; ", brokenRules.Select(r => r.Message)))
    {
        BrokenRules = brokenRules;
    }
}

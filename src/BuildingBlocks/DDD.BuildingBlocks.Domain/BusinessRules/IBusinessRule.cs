namespace BuildingBlocks.Domain.BusinessRules;

/// <summary>
/// Represents a business rule that must be satisfied for an operation to proceed.
/// 
/// Unlike the Specification pattern (which is a "tester" for filtering/querying),
/// IBusinessRule is a "guard" that enforces a policy and provides clear feedback when violated.
/// 
/// Key differences from Specification:
/// - Specification: Returns boolean, used for selection/filtering, declarative style
/// - Business Rule: Returns result with error message, used for validation/enforcement, policy style
/// 
/// Usage:
/// - Enforcing domain invariants within aggregates
/// - Validating specific actions before they occur
/// - Providing clear error messages for rule violations
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Determines whether the business rule is broken (not satisfied).
    /// Returns true if the rule is violated, false if satisfied.
    /// </summary>
    bool IsBroken();

    /// <summary>
    /// The error message to display when the rule is broken.
    /// Should be clear and actionable for the end user.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Optional error code for programmatic handling of rule violations.
    /// </summary>
    string Code => "BUSINESS_RULE_VIOLATION";
}

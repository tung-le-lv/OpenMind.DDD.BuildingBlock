namespace BuildingBlocks.Domain.Exceptions;

/// <summary>
/// Base exception for domain rule violations.
/// Thrown when a domain invariant is violated.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message) : base(message)
    {
        Code = "DOMAIN_ERROR";
    }

    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        Code = "DOMAIN_ERROR";
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base("ENTITY_NOT_FOUND", $"{entityName} with id '{id}' was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public IBusinessRule BrokenRule { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base("BUSINESS_RULE_VIOLATION", brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }
}

/// <summary>
/// Interface for business rules that can be checked.
/// </summary>
public interface IBusinessRule
{
    string Message { get; }
    bool IsBroken();
}

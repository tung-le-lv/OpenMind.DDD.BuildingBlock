namespace BuildingBlocks.Domain.SeedWork;

/// <summary>
/// Marker interface for Domain Services.
/// 
/// Domain Services in DDD (Eric Evans):
/// 1. Represent domain concepts that don't naturally belong to an Entity or Value Object
/// 2. Are stateless
/// 3. Operate on domain objects
/// 4. Express domain concepts in the Ubiquitous Language
/// 
/// When to use Domain Services:
/// - The operation relates to a domain concept that is not a natural part of an Entity or Value Object
/// - The interface is defined in terms of other elements of the domain model
/// - The operation is stateless
/// - The operation involves multiple domain objects
/// 
/// Examples: PricingService, PaymentProcessingService, ShippingCalculator
/// </summary>
public interface IDomainService
{
}

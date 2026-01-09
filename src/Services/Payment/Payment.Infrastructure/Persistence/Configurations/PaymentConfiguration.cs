using BuildingBlocks.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.ValueObjects;

namespace Payment.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Payment Aggregate.
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Aggregates.PaymentAggregate.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.PaymentAggregate.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => PaymentId.From(value))
            .HasColumnName("Id");

        builder.Property(p => p.OrderId)
            .HasConversion(
                id => id.Value,
                value => OrderReference.From(value))
            .HasColumnName("OrderId")
            .IsRequired();

        builder.Property(p => p.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerReference.From(value))
            .HasColumnName("CustomerId")
            .IsRequired();

        // Configure Money Value Object
        builder.OwnsOne(p => p.Amount, amountBuilder =>
        {
            amountBuilder.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            amountBuilder.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Configure PaymentStatus Enumeration
        builder.Property(p => p.Status)
            .HasConversion(
                status => status.Id,
                id => Enumeration.FromValue<PaymentStatus>(id))
            .HasColumnName("StatusId")
            .IsRequired();

        // Configure PaymentMethod Enumeration
        builder.Property(p => p.Method)
            .HasConversion(
                method => method.Id,
                id => Enumeration.FromValue<PaymentMethod>(id))
            .HasColumnName("MethodId")
            .IsRequired();

        // Configure CardDetails Value Object as Owned Entity
        builder.OwnsOne(p => p.CardDetails, cardBuilder =>
        {
            cardBuilder.Property(c => c.Last4Digits)
                .HasColumnName("CardLast4Digits")
                .HasMaxLength(4);

            cardBuilder.Property(c => c.CardType)
                .HasColumnName("CardType")
                .HasMaxLength(50);

            cardBuilder.Property(c => c.ExpiryMonth)
                .HasColumnName("CardExpiryMonth");

            cardBuilder.Property(c => c.ExpiryYear)
                .HasColumnName("CardExpiryYear");

            cardBuilder.Property(c => c.CardHolderName)
                .HasColumnName("CardHolderName")
                .HasMaxLength(200);
        });

        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.Version)
            .IsConcurrencyToken();

        // Indexes
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.CustomerId);

        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}

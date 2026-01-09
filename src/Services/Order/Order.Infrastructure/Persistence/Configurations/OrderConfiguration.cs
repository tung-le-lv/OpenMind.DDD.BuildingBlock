using BuildingBlocks.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Order Aggregate.
/// Maps the rich domain model to the database schema.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Domain.Aggregates.OrderAggregate.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.OrderAggregate.Order> builder)
    {
        builder.ToTable("Orders");

        // Configure OrderId as Value Object
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.From(value))
            .HasColumnName("Id");

        // Configure CustomerId Value Object
        builder.Property(o => o.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerId.From(value))
            .HasColumnName("CustomerId")
            .IsRequired();

        // Configure Address Value Object as Owned Entity
        builder.OwnsOne(o => o.ShippingAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .HasMaxLength(200)
                .IsRequired();

            addressBuilder.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(a => a.State)
                .HasColumnName("ShippingState")
                .HasMaxLength(100);

            addressBuilder.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(a => a.ZipCode)
                .HasColumnName("ShippingZipCode")
                .HasMaxLength(20)
                .IsRequired();
        });

        // Configure OrderStatus Enumeration
        builder.Property(o => o.Status)
            .HasConversion(
                status => status.Id,
                id => Enumeration.FromValue<OrderStatus>(id))
            .HasColumnName("StatusId")
            .IsRequired();

        builder.Property(o => o.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.Version)
            .IsConcurrencyToken();

        // Configure OrderItems relationship
        var itemsNavigation = builder.Metadata.FindNavigation(nameof(Domain.Aggregates.OrderAggregate.Order.OrderItems));
        itemsNavigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events - they're not persisted
        builder.Ignore(o => o.DomainEvents);
        builder.Ignore(o => o.TotalAmount);
    }
}

/// <summary>
/// EF Core configuration for OrderItem Entity.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id)
            .HasConversion(
                id => id.Value,
                value => OrderItemId.From(value))
            .HasColumnName("Id");

        builder.Property(oi => oi.ProductId)
            .HasConversion(
                id => id.Value,
                value => ProductId.From(value))
            .HasColumnName("ProductId")
            .IsRequired();

        builder.Property(oi => oi.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        // Configure Money Value Object for UnitPrice
        builder.OwnsOne(oi => oi.UnitPrice, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("UnitPriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Configure Money Value Object for Discount
        builder.OwnsOne(oi => oi.Discount, discountBuilder =>
        {
            discountBuilder.Property(m => m.Amount)
                .HasColumnName("DiscountAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            discountBuilder.Property(m => m.Currency)
                .HasColumnName("DiscountCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Ignore(oi => oi.DomainEvents);
        builder.Ignore(oi => oi.Total);
    }
}

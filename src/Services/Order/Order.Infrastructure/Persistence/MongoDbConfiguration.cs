using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;

namespace Order.Infrastructure.Persistence;

public static class MongoDbConfiguration
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
            return;

        // Register conventions
        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("DDD Conventions", conventionPack, _ => true);

        // Register Value Object serializers first (before class maps that reference them)
        BsonSerializer.RegisterSerializer(new OrderIdSerializer());
        BsonSerializer.RegisterSerializer(new CustomerIdSerializer());
        BsonSerializer.RegisterSerializer(new OrderItemIdSerializer());
        BsonSerializer.RegisterSerializer(new ProductIdSerializer());
        BsonSerializer.RegisterSerializer(new AddressSerializer());
        BsonSerializer.RegisterSerializer(new MoneySerializer());
        BsonSerializer.RegisterSerializer(new OrderStatusSerializer());

        // Register Order aggregate class map
        BsonClassMap.RegisterClassMap<Domain.Aggregates.OrderAggregate.Order>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.SetIdMember(cm.GetMemberMap("Id"));
            
            // Use creator without calling constructor to avoid _orderItems being overwritten
            cm.SetCreator(() => (Domain.Aggregates.OrderAggregate.Order)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(Domain.Aggregates.OrderAggregate.Order)));
            
            // Unmap any auto-mapped OrderItems property
            var orderItemsProp = typeof(Domain.Aggregates.OrderAggregate.Order).GetProperty("OrderItems");
            if (orderItemsProp != null)
            {
                try { cm.UnmapMember(orderItemsProp); } catch { }
            }
            
            // Map the private _orderItems field
            cm.MapField("_orderItems")
                .SetElementName("orderItems");
        });

        // Register OrderItem class map
        BsonClassMap.RegisterClassMap<OrderItem>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.SetIdMember(cm.GetMemberMap("Id"));
            
            // Use creator without calling constructor
            cm.SetCreator(() => (OrderItem)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(OrderItem)));
        });

        _configured = true;
    }
}

#region Value Object Serializers

public class OrderIdSerializer : SerializerBase<OrderId>
{
    public override OrderId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        return type switch
        {
            BsonType.String => OrderId.From(Guid.Parse(context.Reader.ReadString())),
            BsonType.Binary => OrderId.From(context.Reader.ReadBinaryData().ToGuid()),
            _ => throw new BsonSerializationException($"Cannot deserialize OrderId from {type}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, OrderId value)
    {
        context.Writer.WriteString(value.Value.ToString());
    }
}

public class CustomerIdSerializer : SerializerBase<CustomerId>
{
    public override CustomerId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        return type switch
        {
            BsonType.String => CustomerId.From(Guid.Parse(context.Reader.ReadString())),
            BsonType.Binary => CustomerId.From(context.Reader.ReadBinaryData().ToGuid()),
            _ => throw new BsonSerializationException($"Cannot deserialize CustomerId from {type}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, CustomerId value)
    {
        context.Writer.WriteString(value.Value.ToString());
    }
}

public class OrderItemIdSerializer : SerializerBase<OrderItemId>
{
    public override OrderItemId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        return type switch
        {
            BsonType.String => OrderItemId.From(Guid.Parse(context.Reader.ReadString())),
            BsonType.Binary => OrderItemId.From(context.Reader.ReadBinaryData().ToGuid()),
            _ => throw new BsonSerializationException($"Cannot deserialize OrderItemId from {type}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, OrderItemId value)
    {
        context.Writer.WriteString(value.Value.ToString());
    }
}

public class ProductIdSerializer : SerializerBase<ProductId>
{
    public override ProductId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        return type switch
        {
            BsonType.String => ProductId.From(Guid.Parse(context.Reader.ReadString())),
            BsonType.Binary => ProductId.From(context.Reader.ReadBinaryData().ToGuid()),
            _ => throw new BsonSerializationException($"Cannot deserialize ProductId from {type}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ProductId value)
    {
        context.Writer.WriteString(value.Value.ToString());
    }
}

public class AddressSerializer : SerializerBase<Address>
{
    public override Address Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        context.Reader.ReadStartDocument();
        
        var street = "";
        var city = "";
        var state = "";
        var country = "";
        var zipCode = "";

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);
            switch (name)
            {
                case "street": street = context.Reader.ReadString(); break;
                case "city": city = context.Reader.ReadString(); break;
                case "state": state = context.Reader.ReadString(); break;
                case "country": country = context.Reader.ReadString(); break;
                case "zipCode": zipCode = context.Reader.ReadString(); break;
                default: context.Reader.SkipValue(); break;
            }
        }
        
        context.Reader.ReadEndDocument();
        return new Address(street, city, state, country, zipCode);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Address value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("street");
        context.Writer.WriteString(value.Street);
        context.Writer.WriteName("city");
        context.Writer.WriteString(value.City);
        context.Writer.WriteName("state");
        context.Writer.WriteString(value.State);
        context.Writer.WriteName("country");
        context.Writer.WriteString(value.Country);
        context.Writer.WriteName("zipCode");
        context.Writer.WriteString(value.ZipCode);
        context.Writer.WriteEndDocument();
    }
}

public class MoneySerializer : SerializerBase<Money>
{
    public override Money Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        context.Reader.ReadStartDocument();
        
        decimal amount = 0;
        var currency = "USD";

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);
            switch (name)
            {
                case "amount": amount = (decimal)context.Reader.ReadDouble(); break;
                case "currency": currency = context.Reader.ReadString(); break;
                default: context.Reader.SkipValue(); break;
            }
        }
        
        context.Reader.ReadEndDocument();
        return new Money(amount, currency);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Money value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("amount");
        context.Writer.WriteDouble((double)value.Amount);
        context.Writer.WriteName("currency");
        context.Writer.WriteString(value.Currency);
        context.Writer.WriteEndDocument();
    }
}

public class OrderStatusSerializer : SerializerBase<OrderStatus>
{
    public override OrderStatus Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var statusName = context.Reader.ReadString();
        return BuildingBlocks.Domain.Enumeration.FromDisplayName<OrderStatus>(statusName);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, OrderStatus value)
    {
        context.Writer.WriteString(value.Name);
    }
}

#endregion

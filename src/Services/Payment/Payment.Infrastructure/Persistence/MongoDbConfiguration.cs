using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.ValueObjects;

namespace Payment.Infrastructure.Persistence;

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

        // Register Value Object serializers first
        BsonSerializer.RegisterSerializer(new PaymentIdSerializer());
        BsonSerializer.RegisterSerializer(new OrderReferenceSerializer());
        BsonSerializer.RegisterSerializer(new CustomerReferenceSerializer());
        BsonSerializer.RegisterSerializer(new PaymentMoneySerializer());
        BsonSerializer.RegisterSerializer(new CardDetailsSerializer());
        BsonSerializer.RegisterSerializer(new PaymentStatusSerializer());
        BsonSerializer.RegisterSerializer(new PaymentMethodSerializer());

        // Register Payment aggregate class map
        BsonClassMap.RegisterClassMap<Domain.Aggregates.PaymentAggregate.Payment>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.SetIdMember(cm.GetMemberMap("Id"));
        });

        _configured = true;
    }
}

#region Value Object Serializers

public class PaymentIdSerializer : SerializerBase<PaymentId>
{
    public override PaymentId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        return type switch
        {
            BsonType.String => PaymentId.From(Guid.Parse(context.Reader.ReadString())),
            BsonType.Binary => PaymentId.From(context.Reader.ReadBinaryData().ToGuid()),
            _ => throw new BsonSerializationException($"Cannot deserialize PaymentId from {type}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, PaymentId value)
    {
        context.Writer.WriteString(value.Value.ToString());
    }
}

public class OrderReferenceSerializer : SerializerBase<OrderReference>
{
    public override OrderReference Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        return type switch
        {
            BsonType.String => OrderReference.From(Guid.Parse(context.Reader.ReadString())),
            BsonType.Binary => OrderReference.From(context.Reader.ReadBinaryData().ToGuid()),
            _ => throw new BsonSerializationException($"Cannot deserialize OrderReference from {type}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, OrderReference value)
    {
        context.Writer.WriteString(value.Value.ToString());
    }
}

public class CustomerReferenceSerializer : SerializerBase<CustomerReference>
{
    public override CustomerReference Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        return type switch
        {
            BsonType.String => CustomerReference.From(Guid.Parse(context.Reader.ReadString())),
            BsonType.Binary => CustomerReference.From(context.Reader.ReadBinaryData().ToGuid()),
            _ => throw new BsonSerializationException($"Cannot deserialize CustomerReference from {type}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, CustomerReference value)
    {
        context.Writer.WriteString(value.Value.ToString());
    }
}

public class PaymentMoneySerializer : SerializerBase<Money>
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

public class CardDetailsSerializer : SerializerBase<CardDetails?>
{
    public override CardDetails? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        if (type == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null;
        }

        context.Reader.ReadStartDocument();
        
        var last4Digits = "";
        var cardType = "";
        var expiryMonth = 1;
        var expiryYear = DateTime.UtcNow.Year;
        var cardHolderName = "";

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);
            switch (name)
            {
                case "last4Digits": last4Digits = context.Reader.ReadString(); break;
                case "cardType": cardType = context.Reader.ReadString(); break;
                case "expiryMonth": expiryMonth = context.Reader.ReadInt32(); break;
                case "expiryYear": expiryYear = context.Reader.ReadInt32(); break;
                case "cardHolderName": cardHolderName = context.Reader.ReadString(); break;
                default: context.Reader.SkipValue(); break;
            }
        }
        
        context.Reader.ReadEndDocument();
        return new CardDetails(last4Digits, cardType, expiryMonth, expiryYear, cardHolderName);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, CardDetails? value)
    {
        if (value == null)
        {
            context.Writer.WriteNull();
            return;
        }

        context.Writer.WriteStartDocument();
        context.Writer.WriteName("last4Digits");
        context.Writer.WriteString(value.Last4Digits);
        context.Writer.WriteName("cardType");
        context.Writer.WriteString(value.CardType);
        context.Writer.WriteName("expiryMonth");
        context.Writer.WriteInt32(value.ExpiryMonth);
        context.Writer.WriteName("expiryYear");
        context.Writer.WriteInt32(value.ExpiryYear);
        context.Writer.WriteName("cardHolderName");
        context.Writer.WriteString(value.CardHolderName);
        context.Writer.WriteEndDocument();
    }
}

public class PaymentStatusSerializer : SerializerBase<PaymentStatus>
{
    public override PaymentStatus Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var statusName = context.Reader.ReadString();
        return BuildingBlocks.Domain.Enumeration.FromDisplayName<PaymentStatus>(statusName);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, PaymentStatus value)
    {
        context.Writer.WriteString(value.Name);
    }
}

public class PaymentMethodSerializer : SerializerBase<PaymentMethod>
{
    public override PaymentMethod Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var methodName = context.Reader.ReadString();
        return BuildingBlocks.Domain.Enumeration.FromDisplayName<PaymentMethod>(methodName);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, PaymentMethod value)
    {
        context.Writer.WriteString(value.Name);
    }
}

#endregion

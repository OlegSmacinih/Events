using Amazon.DynamoDBv2.Model;
using Events.Contracts.Enums;
using Events.Contracts.Messages;

namespace Events.Infrastructure.DynamoDb.Mappers;

public static class EventMessageMapper
{
    public static Dictionary<string, AttributeValue> ToDynamoDbItem(EventMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException("message is null and cannot be converted to DynamoDB item.");
        }

        return new Dictionary<string, AttributeValue>
        {
            ["SerialNumber"] = new AttributeValue { S = message.SerialNumber },
            ["TimeStamp"] = new AttributeValue { S = message.TimeStamp.ToUnixTimeSeconds().ToString() },
            ["DeviceType"] = new AttributeValue { S = message.DeviceType.ToString() },
            ["EventType"] = new AttributeValue { S = message.EventType },
            ["SerialNumberEventType"] = new AttributeValue
            {
                S = $"{message.SerialNumber}#{message.EventType}"
            }
        };
    }

    public static EventMessage ToEventMessage(Dictionary<string, AttributeValue> item)
    {
        if (item is null)
        {
            throw new ArgumentNullException("item is null and cannot be converted to EventMessage object");
        }

        return new EventMessage
        {
            SerialNumber = item["SerialNumber"].S,
            TimeStamp = DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(item["TimeStamp"].N)),
            DeviceType = Enum.Parse<DeviceType>(item["DeviceType"].S),
            EventType = item["EventType"].S
        };
    }
}

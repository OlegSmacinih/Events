using Amazon.DynamoDBv2.Model;
using Events.Contracts.Enums;
using Events.Contracts.Messages;

namespace Events.Infrastructure.DynamoDb.Mappers;

public static class EventMessageMapper
{
    public static Dictionary<string, AttributeValue> ToDynamoDbItem(EventMessage message)
    {
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

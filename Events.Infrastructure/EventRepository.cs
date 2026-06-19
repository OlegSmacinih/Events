using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Events.Contracts.Abstractions;
using Events.Contracts.DTOs;
using Events.Contracts.Enums;
using Events.Contracts.Messages;
using Events.Infrastructure.DynamoDb;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Events.Infrastructure;

public class EventRepository : IEventRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbOptions _options;

    public EventRepository(IAmazonDynamoDB dynamoDB, IOptions<DynamoDbOptions> options)
    {
        _dynamoDb = dynamoDB;
        _options = options.Value;
    }    

    public async Task SaveAsync(EventMessage message, CancellationToken cancellationToken)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["SerialNumber"] = new AttributeValue { S = message.SerialNumber },
            ["TimeStamp"] = new AttributeValue { S = message.TimeStamp.ToString() },
            ["DeviceType"] = new AttributeValue { S = message.DeviceType.ToString() },
            ["EventType"] = new AttributeValue { S = message.EventType }
        };

        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = _options.TableName,
            Item = item
        });
    }

    public async Task<PaginatedEventsResult> GetDevicePaginatedEventsInRangeAsync(
        string serialNumber,
        DateTimeOffset from,
        DateTimeOffset to, 
        int limit, 
        string? cursor, 
        CancellationToken cancellationToken)
    {
        //Request Logic
        var request = new QueryRequest
        {
            TableName = _options.TableName,
            KeyConditionExpression = "SerialNumber = :serialNumber AND #timestamp BETWEEN :from AND :to",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#timestamp"] = "TimeStamp"
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":serialNumber"] = new AttributeValue { S = serialNumber },
                [":from"] = new AttributeValue { S = from.ToString() },
                [":to"] = new AttributeValue { S = to.ToString() }
            },
            Limit = limit,
            ScanIndexForward = false
        };

        if (!string.IsNullOrEmpty(cursor))
        {
            request.ExclusiveStartKey = JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
                Encoding.UTF8.GetString(
                    Convert.FromBase64String(cursor)));
        }

        //Response processing
        var response = await _dynamoDb.QueryAsync(request, cancellationToken);

        var events = response.Items
            .Select(item => new EventMessage
            {
                SerialNumber = item["SerialNumber"].S,
                TimeStamp = DateTimeOffset.Parse(item["TimeStamp"].S),
                DeviceType = Enum.Parse<DeviceType>(item["DeviceType"].S),
                EventType = item["EventType"].S
            })
            .ToList();

        var nextCursor = response.LastEvaluatedKey == null 
            ? null 
            : Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(response.LastEvaluatedKey)));

        return new PaginatedEventsResult
        {
            Items = events,
            Cursor = nextCursor
        };
    }

    public async Task<EventMessage?> GetMostRecentEventForTypeAsync(
        string serialNumber, 
        string eventType, 
        CancellationToken cancellationToken)
    {
        var request = new QueryRequest
        {
            TableName = _options.TableName,
            KeyConditionExpression = "SerialNumber = :serialNumber",
            FilterExpression = "EventType = :eventType",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":serialNumber"] = new AttributeValue { S = serialNumber },
                [":eventType"] = new AttributeValue { S = eventType }
            },
            ScanIndexForward = false
        };

        var response = await _dynamoDb.QueryAsync(request, cancellationToken);
        var item = response.Items.FirstOrDefault();

        return item is null ? null : new EventMessage
        {
            SerialNumber = item["SerialNumber"].S,
            TimeStamp = DateTimeOffset.Parse(item["TimeStamp"].S),
            DeviceType = Enum.Parse<DeviceType>(item["DeviceType"].S),
            EventType = item["EventType"].S
        };
    }
}

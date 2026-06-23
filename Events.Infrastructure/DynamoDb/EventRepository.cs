using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Events.Contracts.Abstractions;
using Events.Contracts.DTOs;
using Events.Contracts.Enums;
using Events.Contracts.Messages;
using Events.Infrastructure.DynamoDb.Mappers;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Events.Infrastructure.DynamoDb;

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
        var item = EventMessageMapper.ToDynamoDbItem(message);

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
                [":from"] = new AttributeValue { S = from.ToUnixTimeSeconds().ToString() },
                [":to"] = new AttributeValue { S = to.ToUnixTimeSeconds().ToString() }
            },
            Limit = limit,
            ScanIndexForward = false
        };

        if (!string.IsNullOrEmpty(cursor))
        {
            request.ExclusiveStartKey = ConvertBase64StringToItem(cursor);
        }

        //Response processing
        var response = await _dynamoDb.QueryAsync(request, cancellationToken);

        var events = response.Items
            .Select(item => EventMessageMapper.ToEventMessage(item))
            .ToList();

        var nextCursor = response.LastEvaluatedKey == null
            ? null
            : ConvertItemToBase64String(response.LastEvaluatedKey);

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
            IndexName = DynamoDbIndexes.SerialNumberEventTypeIndex,
            KeyConditionExpression = "SerialNumberEventType = :pk",            
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = $"{serialNumber}#{eventType}" }
            },
            ScanIndexForward = false,
            Limit = 1
        };

        var response = await _dynamoDb.QueryAsync(request, cancellationToken);
        var item = response.Items.FirstOrDefault();

        return item is null ? null : EventMessageMapper.ToEventMessage(item);
    }

    private static string ConvertItemToBase64String(Dictionary<string, AttributeValue> item)
    {
        return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(item)));
    }

    private static Dictionary<string, AttributeValue>? ConvertBase64StringToItem(string base64string)
    {
        return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
                Encoding.UTF8.GetString(
                    Convert.FromBase64String(base64string)));
    }
}

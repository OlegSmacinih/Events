using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Events.Contracts.Abstractions;
using Events.Contracts.Messages;
using Events.Infrastructure.DynamoDb;
using Microsoft.Extensions.Options;

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
}

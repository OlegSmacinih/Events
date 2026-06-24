using Events.Contracts.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Events.Infrastructure.RabbitMq;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqPublisherOptions _options;
    private readonly IRabbitMqConnectionManager _connectionManager;

    public RabbitMqPublisher(
        IOptions<RabbitMqPublisherOptions> options,
        IRabbitMqConnectionManager connectionManager)
    {
        _options = options.Value;
        _connectionManager = connectionManager;
    }

    public async Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken cancellationToken)
    {
        var channel = await _connectionManager.GetPublisherChannelAsync(cancellationToken);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            body: body,
            cancellationToken: cancellationToken);
    }
    
}

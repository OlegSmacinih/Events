using Events.Contracts.Abstractions;
using Events.Contracts.Enums;
using Events.Contracts.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Events.Infrastructure.RabbitMq;

public class RabbitMqConsumer : IRabbitMqConsumer
{
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly RabbitMqConsumerOptions _options;
    private readonly IEventMessageProcessor _messageProcessor;

    public RabbitMqConsumer(
        ILogger<RabbitMqConsumer> logger,
        IOptions<RabbitMqConsumerOptions> options,
        IEventMessageProcessor messageProcessor)
    {
        _logger = logger;
        _options = options.Value;
        _messageProcessor = messageProcessor;
    }

    private async Task DeclareRabbitMq(
        IChannel channel,
        string exchangeName,
        string queueName,
        string routingKey,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: cancellationToken);
    }

    public async Task StartConsumingAsync(
        DeviceType deviceType,
        CancellationToken cancellationToken)
    {       
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        using var connection = await factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var exchangeName = _options.ExchangeName;        
        var queueName = _options.QueueName;
        var routingKey = _options.RoutingKey;

        await DeclareRabbitMq(
            channel,
            exchangeName,
            queueName,
            routingKey,
            cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<EventMessage>(json);

                if (message is null)
                {
                    _logger.LogWarning("Received empty or invalid message");
                    await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                    return;
                }

                _logger.LogInformation($"Received {deviceType} event. SerialNumber={message.SerialNumber}, EventType={message.EventType}, TimeStamp={message.TimeStamp}");
               
                await _messageProcessor.ProcessAsync(message, cancellationToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing {deviceType} message");

                await channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation($"{deviceType} worker started. Queue={queueName}, RoutingKey={routingKey}");

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

}

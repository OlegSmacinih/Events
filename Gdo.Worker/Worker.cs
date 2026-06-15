using Events.Contracts.Abstractions;
using Events.Contracts.Messages;
using Gdo.Worker.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Gdo.Worker;

public class Worker : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<Worker> _logger;
    private readonly IEventRepository _eventRepository;

    public Worker(IOptions<RabbitMqOptions> options, ILogger<Worker> logger, IEventRepository eventRepository)
    {
        _options = options.Value;
        _logger = logger;
        _eventRepository = eventRepository;
    }

    private async Task DeclareRabbitMq(
        IChannel channel, 
        string exchangeName, 
        string queueName, 
        string routingKey, 
        CancellationToken stoppingToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var exchangeName = _options.ExchangeName;
        var queueName = _options.QueueName;
        var routingKey = _options.RoutingKey;

        await DeclareRabbitMq(
            channel, 
            exchangeName,
            queueName,
            routingKey,
            stoppingToken);

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
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                _logger.LogInformation($"Received GDO event. SerialNumber={message.SerialNumber}, EventType={message.EventType}, TimeStamp={message.TimeStamp}");
                
                await _eventRepository.SaveAsync(message);
                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing GDO message");

                await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }                
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation($"GDO worker started. Queue={queueName}, RoutingKey={routingKey}");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}

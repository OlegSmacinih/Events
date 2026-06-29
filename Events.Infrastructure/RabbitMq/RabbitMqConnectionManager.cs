using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Events.Infrastructure.RabbitMq;

public class RabbitMqConnectionManager : IRabbitMqConnectionManager
{
    private readonly RabbitMqPublisherOptions _publihserOptions;
    private readonly RabbitMqConsumerOptions _consumerOptions;

    private IConnection _connection;   
    private IChannel _publisherChannel;    
    private IChannel _consumerChannel;

    public RabbitMqConnectionManager(
        IOptions<RabbitMqPublisherOptions> publisherOptions,
        IOptions<RabbitMqConsumerOptions> consumerOptions)
    {
        _publihserOptions = publisherOptions.Value;
        _consumerOptions = consumerOptions.Value;
    }

    public async Task<IChannel> GetPublisherChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_publisherChannel is not null)
        {
            return _publisherChannel;
        }

        await EnsureConnectionEstablishedAsync(cancellationToken);
        _publisherChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _publisherChannel.ExchangeDeclareAsync(
            exchange: _publihserOptions.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        return _publisherChannel;
    }

    public async Task<IChannel> GetConsumerChannelAsync(CancellationToken cancellationToken)
    {
        if (_consumerChannel is not null)
        {
            return _consumerChannel;
        }

        await EnsureConnectionEstablishedAsync(cancellationToken);
        _consumerChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _consumerChannel.ExchangeDeclareAsync(
            exchange: _consumerOptions.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        await _consumerChannel.QueueDeclareAsync(
            queue: _consumerOptions.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _consumerChannel.QueueBindAsync(
            queue: _consumerOptions.QueueName,
            exchange: _consumerOptions.ExchangeName,
            routingKey: _consumerOptions.RoutingKey,
            cancellationToken: cancellationToken);

        return _consumerChannel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        if (_publisherChannel is not null) await _publisherChannel.DisposeAsync();        
        if (_consumerChannel is not null) await _consumerChannel.DisposeAsync();
    }

    private async Task EnsureConnectionEstablishedAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null) return;

        var factory = new ConnectionFactory
        {
            HostName = _publihserOptions.HostName is not null ? _publihserOptions.HostName : _consumerOptions.HostName,
            UserName = _publihserOptions.UserName is not null ? _publihserOptions.UserName : _consumerOptions.UserName,
            Password = _publihserOptions.Password is not null ? _publihserOptions.Password : _consumerOptions.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
    }
}

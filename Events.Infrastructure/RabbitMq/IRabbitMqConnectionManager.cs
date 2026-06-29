using RabbitMQ.Client;

namespace Events.Infrastructure.RabbitMq;

public interface IRabbitMqConnectionManager : IAsyncDisposable
{
    Task<IChannel> GetPublisherChannelAsync(CancellationToken cancellationToken = default);
    Task<IChannel> GetConsumerChannelAsync(CancellationToken cancellationToken = default);
}

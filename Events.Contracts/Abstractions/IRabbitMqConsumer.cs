using Events.Contracts.Enums;

namespace Events.Contracts.Abstractions;

public interface IRabbitMqConsumer
{
    Task StartConsumingAsync(
        DeviceType deviceType,
        CancellationToken cancellationToken);
}

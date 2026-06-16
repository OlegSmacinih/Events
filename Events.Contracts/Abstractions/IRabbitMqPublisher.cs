namespace Events.Contracts.Abstractions;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken cancellationToken);
}

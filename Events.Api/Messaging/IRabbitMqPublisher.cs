namespace Events.Api.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken cancellationToken);
}

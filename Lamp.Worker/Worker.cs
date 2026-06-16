using Events.Contracts.Abstractions;
using Events.Contracts.Enums;

namespace Lamp.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEventRepository _eventRepository;
    private readonly IRabbitMqConsumer _consumer;

    public Worker(
        ILogger<Worker> logger,
        IEventRepository eventRepository,
        IRabbitMqConsumer consumer)
    {
        _logger = logger;
        _eventRepository = eventRepository;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _consumer.StartConsumingAsync(DeviceType.Lamp, cancellationToken);
    }
}

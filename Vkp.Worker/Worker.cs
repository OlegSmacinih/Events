using Events.Contracts.Abstractions;
using Events.Contracts.Enums;

namespace Vkp.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRabbitMqConsumer _consumer;

    public Worker(
        ILogger<Worker> logger,
        IRabbitMqConsumer consumer)
    {
        _logger = logger;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _consumer.StartConsumingAsync(DeviceType.Vkp, cancellationToken);
    }
}

using Events.Contracts.Abstractions;
using Events.Contracts.Messages;

namespace Lamp.Worker;

class LampEventMessageProcessor : IEventMessageProcessor
{
    private readonly IEventRepository _eventRepository;

    public LampEventMessageProcessor(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task ProcessAsync(EventMessage message, CancellationToken cancellationToken)
    {
        await _eventRepository.SaveAsync(message, cancellationToken);
    }
}

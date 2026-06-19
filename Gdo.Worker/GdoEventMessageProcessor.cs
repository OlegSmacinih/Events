using Events.Contracts.Abstractions;
using Events.Contracts.Messages;

namespace Gdo.Worker;

class GdoEventMessageProcessor : IEventMessageProcessor
{
    private readonly IEventRepository _eventRepository;

    public GdoEventMessageProcessor(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task ProcessAsync(EventMessage message, CancellationToken cancellationToken)
    {
        await _eventRepository.SaveAsync(message, cancellationToken);
    }
}

using Events.Contracts.Abstractions;
using Events.Contracts.Messages;

namespace Vkp.Worker;

class VkpEventMessageProcessor : IEventMessageProcessor
{
    private readonly IEventRepository _eventRepository;

    public VkpEventMessageProcessor(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task ProcessAsync(EventMessage message, CancellationToken cancellationToken)
    {
        await _eventRepository.SaveAsync(message, cancellationToken);
    }
}

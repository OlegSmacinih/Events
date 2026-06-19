using Events.Contracts.Messages;

namespace Events.Contracts.Abstractions;

public interface IEventMessageProcessor
{
    Task ProcessAsync(EventMessage message, CancellationToken cancellationToken);
}
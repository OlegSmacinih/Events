using Events.Contracts.Messages;

namespace Events.Contracts.Abstractions;

public interface IEventRepository
{
    Task SaveAsync(EventMessage message);
}

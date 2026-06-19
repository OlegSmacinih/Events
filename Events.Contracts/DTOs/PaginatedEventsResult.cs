using Events.Contracts.Messages;

namespace Events.Contracts.DTOs;

public class PaginatedEventsResult
{
    public List<EventMessage> Items { get; set; }
    public string? Cursor { get; set; }
}

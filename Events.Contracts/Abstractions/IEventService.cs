using Events.Contracts.DTOs;
using Events.Contracts.Messages;

namespace Events.Contracts.Abstractions;

public interface IEventService
{
    Task CreateEventAsync(
        string serialNumber,
        CreateEventRequest request,
        CancellationToken cancellationToken);

    Task<PaginatedEventsResult> GetDevicePaginatedEventsResultAsync(
        string serialNumber,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        string? cursor,
        CancellationToken cancellationToken);

    Task<EventMessage?> GetMostRecentEventForTypeAsync(
        string serialNumber,
        string eventType,
        CancellationToken cancellationToken);
}

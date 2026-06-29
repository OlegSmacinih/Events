using Events.Contracts.Abstractions;
using Events.Contracts.DTOs;
using Events.Contracts.Enums;
using Events.Contracts.Exceptions;
using Events.Contracts.Messages;
using Events.Contracts.Parsing;
using Events.Contracts.Validation;

namespace Events.Api.Services;

public class EventService : IEventService
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly IEventRepository _eventRepository;

    public EventService(IRabbitMqPublisher publisher, IEventRepository eventRepository)
    {
        _publisher = publisher;
        _eventRepository = eventRepository;
    }

    public async Task CreateEventAsync(
        string serialNumber,
        CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var deviceType = ParseDeviceTypeAndCheckEventType(serialNumber, request.Type);

        var message = new EventMessage
        {
            SerialNumber = serialNumber,
            DeviceType = deviceType,
            EventType = request.Type,
            TimeStamp = DateTimeOffset.UtcNow
        };

        var routingKey = $"{deviceType.ToString().ToLower()}.{request.Type}";

        await _publisher.PublishAsync(message, routingKey, cancellationToken);
    }

    public async Task<PaginatedEventsResult> GetDevicePaginatedEventsResultAsync(
        string serialNumber,
        DateTimeOffset from,
        DateTimeOffset to,
        int limit,
        string? cursor,
        CancellationToken cancellationToken)
    {
        if (from > to)
        {
            throw new BadRequestException("from must be earlier than to");
        }
        if (limit <= 0)
        {
            throw new BadRequestException("limit must be positive number");
        }

        return await _eventRepository.GetDevicePaginatedEventsInRangeAsync(
            serialNumber,
            from,
            to,
            limit,
            cursor,
            cancellationToken);
    }

    public async Task<EventMessage?> GetMostRecentEventForTypeAsync(
        string serialNumber, 
        string eventType, 
        CancellationToken cancellationToken)
    {
        var deviceType = ParseDeviceTypeAndCheckEventType(serialNumber, eventType);

        return await _eventRepository.GetMostRecentEventForTypeAsync(
            serialNumber,
            eventType,
            cancellationToken);
    }

    private DeviceType ParseDeviceTypeAndCheckEventType(string serialNumber, string eventType)
    {
        var deviceType = DeviceTypeParser.Parse(serialNumber);
        if (!DeviceEventValidator.IsValid(deviceType, eventType))
        {
            throw new BadRequestException("Invalid event type for this device.");
        }

        return deviceType;
    }
}

using Events.Contracts.Abstractions;
using Events.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

[ApiController]
[Route("api/devices")]
public class EventsController : ControllerBase
{
    private readonly ILogger<EventsController> _logger;
    private readonly IEventService _eventService;

    public EventsController(ILogger<EventsController> logger, IEventService eventService)
    {
        _logger = logger;
        _eventService = eventService;
    }

    [HttpPost("{serialNumber}/events")]
    public async Task<IActionResult> CreateEvent(
        [FromRoute] string serialNumber, 
        [FromBody] CreateEventRequest request, 
        CancellationToken cancellationToken = default)
    {
        await _eventService.CreateEventAsync(serialNumber, request, cancellationToken);
        return Ok();       
    }

    [HttpGet("{serialNumber}/events")]
    public async Task<IActionResult> GetDevicePaginatedEventsInRange(
        [FromRoute] string serialNumber,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _eventService.GetDevicePaginatedEventsResultAsync(
            serialNumber,
            from,
            to,
            limit,
            cursor,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{serialNumber}/events/latest")]
    public async Task<IActionResult> GetMostRecentEventForType(
        [FromRoute] string serialNumber,
        [FromQuery] string eventType,
        CancellationToken cancellationToken = default)
    {        
        var result = await _eventService.GetMostRecentEventForTypeAsync(
            serialNumber,
            eventType,
            cancellationToken);            

        return Ok(result);       
    }

}

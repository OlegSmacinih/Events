using Events.Contracts.Abstractions;
using Events.Contracts.Messages;
using Events.Contracts.Parsing;
using Events.Contracts.Requests;
using Events.Contracts.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Events.Api.Controllers;

[ApiController]
[Route("api/devices")]
public class EventsController : ControllerBase
{
    private readonly ILogger<EventsController> _logger;
    private readonly IRabbitMqPublisher _publisher;

    public EventsController(ILogger<EventsController> logger, IRabbitMqPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    [HttpPost("{serialNumber}/events")]
    public async Task<IActionResult> CreateEvent(
        [FromRoute] string serialNumber, 
        [FromBody] CreateEventRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceType = DeviceTypeParser.Parse(serialNumber);
            if (!DeviceEventValidator.IsValid(deviceType, request.Type)) return BadRequest("Invalid event type for this device.");

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
        catch (ArgumentException ex)
        {
            return Problem(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem("Some error appeared on the server");
        }

        return Ok();
    }

    [HttpGet("{serialNumber}/events")]
    public async Task<IActionResult> GetPaginatedEventsInRange(
        [FromRoute] string serialNumber,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? limit,
        [FromQuery] string? cursor)
    {
        return NotFound();
    }

    [HttpGet("{serialNumber}/events/latest")]
    public async Task<IActionResult> GetMostRecentEventForType(
        [FromRoute] string serialNumber,
        [FromQuery] string type)
    {
        return NotFound();
    }

}

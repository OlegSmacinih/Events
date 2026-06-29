namespace Events.Contracts.DTOs;

public class CreateEventRequest
{
    public string Type { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

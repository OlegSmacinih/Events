namespace Events.Contracts.Requests;

public class CreateEventRequest
{
    public string Type { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

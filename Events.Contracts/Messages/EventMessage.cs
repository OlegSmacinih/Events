using Events.Contracts.Enums;

namespace Events.Contracts.Messages;

public class EventMessage
{
    public string SerialNumber { get; set; }
    public DeviceType DeviceType { get; set; }
    public string EventType { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}

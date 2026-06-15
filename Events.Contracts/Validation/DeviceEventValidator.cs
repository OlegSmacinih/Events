using Events.Contracts.Enums;

namespace Events.Contracts.Validation;

public static class DeviceEventValidator
{
    private static readonly Dictionary<DeviceType, HashSet<string>> possibleTypes = new()
    {
        [DeviceType.Gdo] = new()
        {
            "door_opened",
            "door_closed",
            "online",
            "offline"
        },
        [DeviceType.Lamp] = new()
        {
            "on",
            "off",
            "online",
            "offline"
        },
        [DeviceType.Vkp] = new()
        {
            "motion_detected",
            "door_opened",
            "door_closed",
            "online",
            "offline"
        }
    };

    public static bool IsValid(DeviceType deviceType, string eventType)
    {
        if (possibleTypes.TryGetValue(deviceType, out var eventTypes))
        {
            return eventTypes.Contains(eventType);
        }
        
        return false;
    }
}

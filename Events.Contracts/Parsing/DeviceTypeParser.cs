using Events.Contracts.Enums;

namespace Events.Contracts.Parsing;

public static class DeviceTypeParser
{
    public static DeviceType Parse(string serialNumber)
    {
        if (serialNumber.StartsWith("GDO_", StringComparison.OrdinalIgnoreCase)) return DeviceType.Gdo;
        if (serialNumber.StartsWith("LMP_", StringComparison.OrdinalIgnoreCase)) return DeviceType.Lamp;
        if (serialNumber.StartsWith("VKP_", StringComparison.OrdinalIgnoreCase)) return DeviceType.Vkp;

        throw new ArgumentException($"Unknown device type for serial number '{serialNumber}'");
    }
}

using System.Text.Json.Serialization;

namespace Events.Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceType
{
    Gdo,
    Lamp,
    Vkp
}

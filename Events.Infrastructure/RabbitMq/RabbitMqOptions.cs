namespace Events.Infrastructure.RabbitMq;

public class RabbitMqOptions
{
    public string HostName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ExchangeName { get; set; }

    public string? GdoQueue { get; set; }
    public string? GdoRoutingKey { get; set; }

    public string? LampQueue { get; set; }
    public string? LampRoutingKey { get; set; }

    public string? VkpQueue { get; set; }
    public string? VkpRoutingKey { get; set; }

}

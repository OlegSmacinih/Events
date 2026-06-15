namespace Events.Infrastructure.Configuration;

public class DynamoDbOptions
{
    public string ServiceUrl { get; set; }
    public string TableName { get; set; }
    public string Region { get; set; }
}

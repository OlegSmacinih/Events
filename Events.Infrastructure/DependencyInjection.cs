using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Events.Contracts.Abstractions;
using Events.Infrastructure.Configuration;
using Events.Infrastructure.DynamoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DynamoDbOptions>(configuration.GetSection("DynamoDb"));

        services.AddSingleton<IAmazonDynamoDB>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DynamoDbOptions>>().Value;
            var credentials = new BasicAWSCredentials("fake", "fake"); //fake because of local using

            return new AmazonDynamoDBClient(
                credentials,
                new AmazonDynamoDBConfig
                {
                    ServiceURL = options.ServiceUrl
                });
        });

        services.AddSingleton<IEventRepository, EventRepository>();

        return services;
    }
}

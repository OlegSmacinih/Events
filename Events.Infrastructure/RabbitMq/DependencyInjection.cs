using Events.Contracts.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Events.Infrastructure.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqPublishInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqPublisherOptions>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();        

        return services;
    }

    public static IServiceCollection AddRabbitMqConsumeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqConsumerOptions>(configuration.GetSection("RabbitMq"));        
        services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>();

        return services;
    }
}

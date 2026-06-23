using Events.Contracts.Abstractions;
using Events.Infrastructure.DynamoDb;
using Events.Infrastructure.RabbitMq;
using Vkp.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqConsumeInfrastructure(builder.Configuration);
builder.Services.AddDynamoDbInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IEventMessageProcessor, VkpEventMessageProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

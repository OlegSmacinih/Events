using Events.Contracts.Abstractions;
using Events.Infrastructure;
using Events.Infrastructure.DynamoDb;
using Events.Infrastructure.RabbitMq;
using Lamp.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqConsumeInfrastructure(builder.Configuration);
builder.Services.AddDynamoDbInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IEventMessageProcessor, LampEventMessageProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

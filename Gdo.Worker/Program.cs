using Events.Contracts.Abstractions;
using Events.Infrastructure;
using Events.Infrastructure.DynamoDb;
using Events.Infrastructure.RabbitMq;
using Gdo.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRabbitMqConsumeInfrastructure(builder.Configuration);
builder.Services.AddDynamoDbInfrastructure(builder.Configuration);
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventMessageProcessor, GdoEventMessageProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

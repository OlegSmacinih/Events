using Events.Contracts.Abstractions;
using Events.Infrastructure.DynamoDb;
using Events.Infrastructure.RabbitMq;
using Gdo.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqPublisherOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddRabbitMqConsumeInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IEventMessageProcessor, GdoEventMessageProcessor>();

builder.Services.AddDynamoDbInfrastructure(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

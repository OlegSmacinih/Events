using Events.Infrastructure.DynamoDb;
using Events.Infrastructure.RabbitMq;
using Vkp.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqPublisherOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddRabbitMqConsumeInfrastructure(builder.Configuration);
builder.Services.AddDynamoDbInfrastructure(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

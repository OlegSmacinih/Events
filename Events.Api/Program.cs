using Events.Api.Extensions;
using Events.Api.Middlewares;
using Events.Api.Services;
using Events.Contracts.Abstractions;
using Events.Infrastructure;
using Events.Infrastructure.DynamoDb;
using Events.Infrastructure.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwagger();

builder.Services.AddRabbitMqPublishInfrastructure(builder.Configuration);
builder.Services.AddDynamoDbInfrastructure(builder.Configuration);
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();

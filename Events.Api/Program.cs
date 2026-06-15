using Events.Api.Extensions;
using Events.Api.Messaging;
using Events.Contracts.Messages;
using Events.Contracts.Parsing;
using Events.Contracts.Requests;
using Events.Contracts.Validation;
using Gdo.Worker.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("api/devices/{serialNumber}/events",
    async (
        string serialNumber,
        CreateEventRequest request,
        IRabbitMqPublisher publisher,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var deviceType = DeviceTypeParser.Parse(serialNumber);
            if (!DeviceEventValidator.IsValid(deviceType, request.Type)) return Results.BadRequest("Invalid event type for this device.");

            var message = new EventMessage
            {
                SerialNumber = serialNumber,
                DeviceType = deviceType,
                EventType = request.Type,
                TimeStamp = DateTimeOffset.UtcNow
            };

            var routingKey = $"{deviceType.ToString().ToLower()}.{request.Type}";

            await publisher.PublishAsync(message, routingKey, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Results.InternalServerError(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.InternalServerError("Some error appeared on the server");
        }

        return Results.Accepted();
    });

app.MapGet("api/devices/{serialNumber}/events",
    async (string serialNumber, DateTimeOffset? from, DateTimeOffset? to, int? limit, string? cursor) =>
    {
        return Results.NotFound();
    });

app.MapGet("api/devices/{serialNumber}/events/latest",
    async (string serialNumber, string type) =>
    {
        return Results.NotFound();
    });

app.Run();

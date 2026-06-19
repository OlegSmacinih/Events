using Events.Api.Extensions;
using Events.Api.Services;
using Events.Infrastructure.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwagger();

builder.Services.AddRabbitMqPublishInfrastructure(builder.Configuration);
builder.Services.AddScoped<EventService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();

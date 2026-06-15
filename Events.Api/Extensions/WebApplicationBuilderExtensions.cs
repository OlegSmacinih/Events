namespace Events.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo 
            { 
                Title = "Events.Api", 
                Version = "v1" 
            });
        });

        return services;
    }
}

using SaveYourNote.Application.Interfaces;
using SaveYourNote.Application.UseCases.ProcessMessage;
using SaveYourNote.Infrastructure.Logging;

namespace SaveYourNote.Api.Extensions;

/// <summary>
/// Extension methods for configuring services in the DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the DI container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IMessageService, ProcessMessageHandler>();

        // Register infrastructure services
        services.AddSingleton<IMessageLogger, ConsoleMessageLogger>();

        return services;
    }

    /// <summary>
    /// Configures CORS policies
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        // TODO: Update CORS policy as per your requirements
        /*
        CORS is configured with AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(),
        which effectively disables browser cross-origin protections for all callers.
        If this API will be exposed beyond local development,
        restrict origins/headers/methods via configuration (e.g., an allowlist)
        and consider separate policies per environment.
        */
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins("https://your-allowed-origin.com") // Replace with your allowed origin
                    .WithMethods("GET", "POST") // Specify allowed methods
                    .WithHeaders("Content-Type", "Authorization"); // Specify allowed headers
            });
        });

        return services;
    }
}

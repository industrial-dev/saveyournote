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
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        return services;
    }
}

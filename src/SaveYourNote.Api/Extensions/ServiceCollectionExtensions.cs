using SaveYourNote.Application.Interfaces;
using SaveYourNote.Application.UseCases.ProcessMessage;

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

        return services;
    }

    /// <summary>
    /// Configures CORS policies
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        /*
        CORS applies only to browser-based requests. Server-to-server calls (e.g.,
        WhatsApp/Meta webhooks) are not subject to CORS, so they do NOT need to be
        listed here. We therefore scope CORS to local development tools (e.g.,
        Swagger UI, REST clients running in the browser) and leave other origins
        blocked by default.

        If you later expose a web client, add its origin to the allowlist below
        or move the list to configuration (appsettings.json) per environment.
        */
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    // Allow localhost origins with any port (HTTP/HTTPS)
                    .SetIsOriginAllowed(origin =>
                    {
                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        {
                            return false;
                        }

                        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
                    })
                    // Allow typical HTTP verbs used by local tooling
                    .WithMethods("GET", "POST")
                    // Allow common headers sent by local tools and browsers
                    .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                    // If you use cookies or auth headers from a browser, keep this enabled
                    .AllowCredentials();
            });
        });

        return services;
    }
}

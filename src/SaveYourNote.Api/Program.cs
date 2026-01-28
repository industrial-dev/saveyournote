using SaveYourNote.Api.Extensions;
using SaveYourNote.Api.Middleware;
using Serilog;
using Serilog.Events;

// Configure Serilog with two-stage initialization
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from appsettings.json
    builder.Services.AddSerilog(
        (services, lc) =>
            lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
    );

    // Add services to the container
    builder.Services.AddControllers();

    // Add Swashbuckle for Swagger UI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(
            "v1",
            new()
            {
                Version = "v1",
                Title = "SaveYourNote API",
                Description =
                    "API inteligente para guardar y clasificar automáticamente información enviada a través de WhatsApp. Procesa mensajes de texto y audio, utiliza IA para análisis de contenido y almacena de forma organizada.",
                Contact = new()
                {
                    Name = "SaveYourNote Team",
                    Url = new Uri("https://github.com/yourusername/saveyournote"),
                },
                License = new()
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT"),
                },
            }
        );

        // Include XML comments for documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Add application services
    builder.Services.AddApplicationServices();

    // Add CORS
    builder.Services.AddCorsConfiguration();

    // Configure Kestrel to listen on port 5001
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5001);
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        // Swagger UI available at root
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SaveYourNote API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "SaveYourNote API - Documentation";
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
        });
    }

    // Use Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });

    // Use global exception handling middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseCors();

    app.MapControllers();

    // Health check endpoint
    app.MapGet(
            "/health",
            () =>
                Results.Ok(
                    new
                    {
                        status = "healthy",
                        timestamp = DateTime.UtcNow,
                        service = "SaveYourNote API",
                        version = "1.0.0",
                        environment = app.Environment.EnvironmentName,
                    }
                )
        )
        .WithName("HealthCheck")
        .WithTags("Health")
        .WithSummary("Health Check")
        .WithDescription("Verifica el estado del servicio API")
        .Produces<object>(StatusCodes.Status200OK);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

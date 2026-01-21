using SaveYourNote.Api.Extensions;
using SaveYourNote.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

app.MapControllers();

// Add a simple health check endpoint
app.MapGet(
    "/health",
    () =>
        Results.Ok(
            new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "SaveYourNote API",
            }
        )
);

app.Run();

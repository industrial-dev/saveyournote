using SaveYourNote.Api.Configuration;
using SaveYourNote.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure settings
builder.Services.Configure<WhatsAppSettings>(
    builder.Configuration.GetSection(WhatsAppSettings.SectionName));
builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection(StorageSettings.SectionName));
builder.Services.Configure<TranscriptionSettings>(
    builder.Configuration.GetSection(TranscriptionSettings.SectionName));

// Add controllers
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddOpenApi();

// Register application services
builder.Services.AddScoped<IMessageClassificationService, MessageClassificationService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IMessageProcessorService, MessageProcessorService>();

// Register HTTP clients for external services
builder.Services.AddHttpClient<IAudioTranscriptionService, AudioTranscriptionService>();
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();

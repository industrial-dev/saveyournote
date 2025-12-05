using Microsoft.Extensions.Options;
using SaveYourNote.Api.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SaveYourNote.Api.Services;

public interface IAudioTranscriptionService
{
    Task<string> TranscribeAsync(byte[] audioData, string mimeType);
    Task<string> TranscribeAsync(Stream audioStream, string mimeType);
}

public class AudioTranscriptionService : IAudioTranscriptionService
{
    private readonly TranscriptionSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AudioTranscriptionService> _logger;

    public AudioTranscriptionService(
        IOptions<TranscriptionSettings> settings,
        HttpClient httpClient,
        ILogger<AudioTranscriptionService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> TranscribeAsync(byte[] audioData, string mimeType)
    {
        using var stream = new MemoryStream(audioData);
        return await TranscribeAsync(stream, mimeType);
    }

    public async Task<string> TranscribeAsync(Stream audioStream, string mimeType)
    {
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            _logger.LogWarning("Transcription API key not configured. Returning placeholder.");
            return "[Transcription not available - API key not configured]";
        }

        try
        {
            return _settings.Provider?.ToLowerInvariant() switch
            {
                "openai" => await TranscribeWithOpenAiAsync(audioStream, mimeType),
                _ => await TranscribeWithOpenAiAsync(audioStream, mimeType)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio");
            return $"[Transcription failed: {ex.Message}]";
        }
    }

    private async Task<string> TranscribeWithOpenAiAsync(Stream audioStream, string mimeType)
    {
        var extension = GetExtensionFromMimeType(mimeType);
        var fileName = $"audio{extension}";

        using var content = new MultipartFormDataContent();
        
        var streamContent = new StreamContent(audioStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(_settings.Model ?? "whisper-1"), "model");

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/audio/transcriptions",
            content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OpenAiTranscriptionResponse>(responseJson);

        _logger.LogInformation("Successfully transcribed audio");
        return result?.Text ?? "[Empty transcription]";
    }

    private static string GetExtensionFromMimeType(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            "audio/ogg" or "audio/ogg; codecs=opus" => ".ogg",
            "audio/mpeg" => ".mp3",
            "audio/mp4" => ".m4a",
            "audio/wav" => ".wav",
            "audio/webm" => ".webm",
            _ => ".ogg"
        };
    }

    private class OpenAiTranscriptionResponse
    {
        public string? Text { get; set; }
    }
}

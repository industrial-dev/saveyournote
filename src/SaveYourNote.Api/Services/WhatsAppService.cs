using Microsoft.Extensions.Options;
using SaveYourNote.Api.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SaveYourNote.Api.Services;

public interface IWhatsAppService
{
    Task<byte[]> DownloadMediaAsync(string mediaId);
    Task SendTextMessageAsync(string to, string message);
}

public class WhatsAppService : IWhatsAppService
{
    private readonly WhatsAppSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(
        IOptions<WhatsAppSettings> settings,
        HttpClient httpClient,
        ILogger<WhatsAppService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
        
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        if (!string.IsNullOrEmpty(_settings.AccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
        }
    }

    public async Task<byte[]> DownloadMediaAsync(string mediaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaId);

        try
        {
            // First, get the media URL
            var mediaUrl = await GetMediaUrlAsync(mediaId);
            
            // Then download the media
            return await DownloadFromUrlAsync(mediaUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download media {MediaId}", mediaId);
            throw;
        }
    }

    private async Task<string> GetMediaUrlAsync(string mediaId)
    {
        var apiUrl = $"{_settings.BaseUrl}/{_settings.ApiVersion}/{mediaId}";
        
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var mediaInfo = JsonSerializer.Deserialize<MediaInfoResponse>(content);
        
        if (string.IsNullOrEmpty(mediaInfo?.Url))
        {
            throw new InvalidOperationException($"Could not retrieve URL for media {mediaId}");
        }
        
        return mediaInfo.Url;
    }

    private async Task<byte[]> DownloadFromUrlAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task SendTextMessageAsync(string to, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var apiUrl = $"{_settings.BaseUrl}/{_settings.ApiVersion}/{_settings.PhoneNumberId}/messages";
        
        var payload = new
        {
            messaging_product = "whatsapp",
            to = to,
            type = "text",
            text = new { body = message }
        };
        
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(apiUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send message. Status: {Status}, Response: {Response}", 
                response.StatusCode, errorContent);
            throw new InvalidOperationException($"Failed to send WhatsApp message: {response.StatusCode}");
        }
        
        _logger.LogInformation("Successfully sent message to {To}", to);
    }

    private class MediaInfoResponse
    {
        public string? Url { get; set; }
        public string? MimeType { get; set; }
        public string? Sha256 { get; set; }
        public int FileSize { get; set; }
        public string? Id { get; set; }
        public string? MessagingProduct { get; set; }
    }
}

using SaveYourNote.Api.Models;

namespace SaveYourNote.Api.Services;

public interface IMessageProcessorService
{
    Task ProcessWebhookAsync(WhatsAppWebhookPayload payload);
}

public class MessageProcessorService : IMessageProcessorService
{
    private readonly IMessageClassificationService _classificationService;
    private readonly IStorageService _storageService;
    private readonly IAudioTranscriptionService _transcriptionService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<MessageProcessorService> _logger;

    public MessageProcessorService(
        IMessageClassificationService classificationService,
        IStorageService storageService,
        IAudioTranscriptionService transcriptionService,
        IWhatsAppService whatsAppService,
        ILogger<MessageProcessorService> logger)
    {
        _classificationService = classificationService;
        _storageService = storageService;
        _transcriptionService = transcriptionService;
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    public async Task ProcessWebhookAsync(WhatsAppWebhookPayload payload)
    {
        if (payload.Entry == null) return;

        foreach (var entry in payload.Entry)
        {
            if (entry.Changes == null) continue;

            foreach (var change in entry.Changes)
            {
                if (change.Value?.Messages == null) continue;

                foreach (var message in change.Value.Messages)
                {
                    await ProcessMessageAsync(message);
                }
            }
        }
    }

    private async Task ProcessMessageAsync(WhatsAppMessage message)
    {
        try
        {
            _logger.LogInformation("Processing message of type {Type} from {From}", 
                message.Type, message.From);

            switch (message.Type?.ToLowerInvariant())
            {
                case "text":
                    await ProcessTextMessageAsync(message);
                    break;
                case "audio":
                    await ProcessAudioMessageAsync(message);
                    break;
                case "image":
                case "video":
                case "document":
                    await ProcessMediaMessageAsync(message);
                    break;
                default:
                    _logger.LogWarning("Unsupported message type: {Type}", message.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
        }
    }

    private async Task ProcessTextMessageAsync(WhatsAppMessage message)
    {
        var textContent = message.Text?.Body;
        if (string.IsNullOrEmpty(textContent)) return;

        var classified = _classificationService.ClassifyMessage(textContent, message.From);
        await _storageService.SaveMessageAsync(classified);

        _logger.LogInformation("Text message classified as {Category} and saved", classified.Category);
    }

    private async Task ProcessAudioMessageAsync(WhatsAppMessage message)
    {
        var audio = message.Audio;
        if (audio?.Id == null) return;

        try
        {
            // Download audio from WhatsApp
            var audioData = await _whatsAppService.DownloadMediaAsync(audio.Id);
            var mimeType = audio.MimeType ?? "audio/ogg";
            
            // Transcribe the audio
            var transcription = await _transcriptionService.TranscribeAsync(audioData, mimeType);
            
            // Save audio file with transcription
            var fileName = $"audio_{message.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.ogg";
            await _storageService.SaveAudioAsync(fileName, audioData, transcription);

            // Also classify and save the transcription as a message
            if (!string.IsNullOrEmpty(transcription) && !transcription.StartsWith('['))
            {
                var classified = _classificationService.ClassifyMessage(transcription, message.From);
                classified.Category = MessageCategory.Audio; // Override to keep audio category
                await _storageService.SaveMessageAsync(classified);
            }

            _logger.LogInformation("Audio message processed and transcribed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audio message {MessageId}", message.Id);
        }
    }

    private async Task ProcessMediaMessageAsync(WhatsAppMessage message)
    {
        var caption = message.Type?.ToLowerInvariant() switch
        {
            "image" => message.Image?.Caption,
            "video" => message.Video?.Caption,
            "document" => message.Document?.Filename,
            _ => null
        };

        if (string.IsNullOrEmpty(caption))
        {
            _logger.LogInformation("Media message without caption, skipping classification");
            return;
        }

        var classified = _classificationService.ClassifyMessage(caption, message.From);
        await _storageService.SaveMessageAsync(classified);

        _logger.LogInformation("Media message with caption classified as {Category}", classified.Category);
    }
}

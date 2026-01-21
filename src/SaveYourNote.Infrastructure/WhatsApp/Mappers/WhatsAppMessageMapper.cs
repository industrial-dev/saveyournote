using ErrorOr;
using SaveYourNote.Application.UseCases.ProcessMessage;
using SaveYourNote.Domain.Enums;
using SaveYourNote.Infrastructure.WhatsApp.DTOs;

namespace SaveYourNote.Infrastructure.WhatsApp.Mappers;

/// <summary>
/// Maps WhatsApp-specific DTOs to domain entities
/// </summary>
public static class WhatsAppMessageMapper
{
    /// <summary>
    /// Maps a WhatsApp webhook DTO to a ProcessMessageCommand
    /// </summary>
    /// <param name="webhookDto">The WhatsApp webhook payload</param>
    /// <returns>ErrorOr result containing ProcessMessageCommand or errors</returns>
    public static ErrorOr<ProcessMessageCommand> ToCommand(WhatsAppWebhookDto webhookDto)
    {
        if (webhookDto is null)
        {
            return Error.Validation("WhatsApp.InvalidPayload", "Webhook payload is null");
        }

        if (webhookDto.Entry is null || webhookDto.Entry.Count == 0)
        {
            return Error.Validation("WhatsApp.NoEntries", "No entries in webhook payload");
        }

        var entry = webhookDto.Entry[0];
        if (entry.Changes is null || entry.Changes.Count == 0)
        {
            return Error.Validation("WhatsApp.NoChanges", "No changes in webhook entry");
        }

        var change = entry.Changes[0];
        if (change.Value?.Messages is null || change.Value.Messages.Count == 0)
        {
            return Error.Validation("WhatsApp.NoMessages", "No messages in webhook change");
        }

        var whatsAppMessage = change.Value.Messages[0];

        // Parse timestamp (Unix timestamp in seconds)
        if (!long.TryParse(whatsAppMessage.Timestamp, out var unixTimestamp))
        {
            return Error.Validation("WhatsApp.InvalidTimestamp", "Invalid timestamp format");
        }

        var timestamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;

        // Map based on message type
        return whatsAppMessage.Type.ToLowerInvariant() switch
        {
            "text" => MapTextMessage(whatsAppMessage, timestamp),
            "audio" => MapAudioMessage(whatsAppMessage, timestamp),
            _ => Error.Validation("WhatsApp.UnsupportedType", $"Unsupported message type: {whatsAppMessage.Type}")
        };
    }

    private static ErrorOr<ProcessMessageCommand> MapTextMessage(
        WhatsAppMessageDto whatsAppMessage,
        DateTime timestamp)
    {
        if (whatsAppMessage.Text is null || string.IsNullOrWhiteSpace(whatsAppMessage.Text.Body))
        {
            return Error.Validation("WhatsApp.EmptyText", "Text message body is empty");
        }

        return new ProcessMessageCommand(
            MessageId: whatsAppMessage.Id,
            SenderId: whatsAppMessage.From,
            Content: whatsAppMessage.Text.Body,
            Type: MessageType.Text,
            Source: MessageSource.WhatsApp,
            Timestamp: timestamp
        );
    }

    private static ErrorOr<ProcessMessageCommand> MapAudioMessage(
        WhatsAppMessageDto whatsAppMessage,
        DateTime timestamp)
    {
        if (whatsAppMessage.Audio is null)
        {
            return Error.Validation("WhatsApp.NoAudioData", "Audio message has no audio data");
        }

        return new ProcessMessageCommand(
            MessageId: whatsAppMessage.Id,
            SenderId: whatsAppMessage.From,
            Content: whatsAppMessage.Audio.Id, // Audio ID is the content for audio messages
            Type: MessageType.Audio,
            Source: MessageSource.WhatsApp,
            Timestamp: timestamp,
            AudioMimeType: whatsAppMessage.Audio.MimeType,
            AudioSha256: whatsAppMessage.Audio.Sha256
        );
    }
}

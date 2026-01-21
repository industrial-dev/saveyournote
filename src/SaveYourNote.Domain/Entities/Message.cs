using SaveYourNote.Domain.Enums;
using SaveYourNote.Domain.ValueObjects;

namespace SaveYourNote.Domain.Entities;

/// <summary>
/// Core domain entity representing a message from any source
/// This entity is technology-agnostic and not tied to any specific messaging platform
/// </summary>
public sealed class Message
{
    public MessageId Id { get; private set; }
    public SenderId SenderId { get; private set; }
    public MessageType Type { get; private set; }
    public MessageSource Source { get; private set; }
    public DateTime Timestamp { get; private set; }

    // Content can be either text or audio
    public TextContent? TextContent { get; private set; }
    public AudioContent? AudioContent { get; private set; }

    // Private constructor to enforce factory methods
    private Message(
        MessageId id,
        SenderId senderId,
        MessageType type,
        MessageSource source,
        DateTime timestamp,
        TextContent? textContent,
        AudioContent? audioContent
    )
    {
        Id = id;
        SenderId = senderId;
        Type = type;
        Source = source;
        Timestamp = timestamp;
        TextContent = textContent;
        AudioContent = audioContent;
    }

    /// <summary>
    /// Creates a text message
    /// </summary>
    public static Message? CreateTextMessage(
        MessageId id,
        SenderId senderId,
        MessageSource source,
        DateTime timestamp,
        TextContent textContent
    )
    {
        if (id is null || senderId is null || textContent is null)
        {
            return null;
        }

        if (timestamp > DateTime.UtcNow.AddMinutes(5)) // Allow 5 min clock skew
        {
            return null;
        }

        return new Message(id, senderId, MessageType.Text, source, timestamp, textContent, null);
    }

    /// <summary>
    /// Creates an audio message
    /// </summary>
    public static Message? CreateAudioMessage(
        MessageId id,
        SenderId senderId,
        MessageSource source,
        DateTime timestamp,
        AudioContent audioContent
    )
    {
        if (id is null || senderId is null || audioContent is null)
        {
            return null;
        }

        if (timestamp > DateTime.UtcNow.AddMinutes(5)) // Allow 5 min clock skew
        {
            return null;
        }

        return new Message(id, senderId, MessageType.Audio, source, timestamp, null, audioContent);
    }

    /// <summary>
    /// Gets the content as a string for display purposes
    /// </summary>
    public string GetContentDisplay()
    {
        return Type switch
        {
            MessageType.Text => TextContent?.Value ?? "[Empty text]",
            MessageType.Audio => AudioContent?.ToString() ?? "[Empty audio]",
            _ => "[Unknown content type]",
        };
    }

    /// <summary>
    /// Validates that the message is in a consistent state
    /// </summary>
    public bool IsValid()
    {
        return Type switch
        {
            MessageType.Text => TextContent is not null && AudioContent is null,
            MessageType.Audio => AudioContent is not null && TextContent is null,
            _ => false,
        };
    }
}
